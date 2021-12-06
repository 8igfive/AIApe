
from typing import Dict, Iterable, List
from utils.Utils import ensure_dir, log, pcat
import wx
from wx.core import LanguageInfo, PAPER_10X11, Size
from utils.Config import Path
import os
import json

class Model:
    def __init__(self, 
        title: str, 
        remarks: str, 
        origin_tags: Iterable[str]=None, 
        auto_tags: Iterable[str]=None, 
        add_tags: Iterable[str]=None,
        delete_tags: Iterable[str]=None
        ):
        self.title = title
        self.remarks = remarks
        self._origin_tags = set(origin_tags) if origin_tags is not None else set()
        self._auto_tags = set(auto_tags) if auto_tags is not None else set()
        self.add_tags = set(add_tags) if add_tags is not None else set()
        self.delete_tags = set(delete_tags) if delete_tags is not None else set()

    def has_tag(self, tag_name: str):
        if tag_name in self.delete_tags:
            return False
        return tag_name in self._origin_tags or tag_name in self._auto_tags or tag_name in self.add_tags
    
    def delete_tag(self, tag_name: str):
        if tag_name in self.add_tags:
            self.add_tags.remove(tag_name)
        if tag_name in self._origin_tags or tag_name in self._auto_tags:
            self.delete_tags.add(tag_name)
    
    def add_tag(self, tag_name: str):
        if tag_name in self.delete_tags:
            self.delete_tags.remove(tag_name)
        if tag_name not in self._origin_tags and tag_name not in self._auto_tags:
            self.add_tags.add(tag_name)

class ModelFactory:
    def __init__(self, lang: str):
        self.lang = lang
        self.questions = os.listdir(pcat(pcat(Path.Scripte_DataManager_Datas, "pure"), lang))
        select = pcat(Path.Scripte_DataManager_Datas, "select")
        select_lang = pcat(select, self.lang)
        ensure_dir(select_lang)

    def get_max_index(self):
        return len(self.questions) - 1

    def _pure_lang_question(self, index: int) -> str:
        pure = pcat(Path.Scripte_DataManager_Datas, "pure")
        pure_lang = pcat(pure, self.lang)
        return pcat(pure_lang, self.questions[index])

    def _auto_tag_lang_question(self, index: int) -> str:
        auto_tag = pcat(Path.Scripte_DataManager_Datas, "auto_tag")
        auto_tag_lang = pcat(auto_tag, self.lang)
        return pcat(auto_tag_lang, self.questions[index])

    def _select_lang_question(self, index: int) -> int:
        select = pcat(Path.Scripte_DataManager_Datas, "select")
        select_lang = pcat(select, self.lang)
        return pcat(select_lang, self.questions[index])

    def __setitem__(self, index: int, value: Model):
        select_lang_question = self._select_lang_question(index)
        json_content = json.dumps({"add_tags" : list(value.add_tags), "delete_tags" : list(value.delete_tags)}, ensure_ascii=False)
        log(f"{json_content} -> {select_lang_question}")
        with open(select_lang_question, 'w', encoding='utf8') as f:
            f.write(json_content)

    def __getitem__(self, index: int)->Model:
        with open(self._pure_lang_question(index), 'r', encoding='utf8') as f:
            pure_data = json.load(f)
        title = pure_data["title"]
        remarks = pure_data["remarks"]
        origin_tags = pure_data["tags"]
        auto_tags = None
        auto_tag_lang_question = self._auto_tag_lang_question(index)
        if os.path.exists(auto_tag_lang_question):
            with open(auto_tag_lang_question, 'r', encoding='utf8') as f:
                auto_tags = json.load(f)
        add_tags = None
        delete_tags = None
        select_lang_question = self._select_lang_question(index)
        if os.path.exists(select_lang_question):
            with open(select_lang_question, 'r', encoding='utf8') as f:
                select_data = json.load(f)
            add_tags = select_data["add_tags"]
            delete_tags = select_data["delete_tags"]
        return Model(title, remarks, origin_tags, auto_tags, add_tags, delete_tags)

class _EventHandler:
    def __init__(self, func, *args) -> None:
        self.args = args
        self.func = func
    
    def __call__(self, event):
        self.func(*self.args)
        
        


class MyFrame(wx.Frame):
    def __init__(self, parent, id, language: str, tags: Dict[str, List[str]]):
        super().__init__(parent, id, "Select Tags", size=(1024, 600))
        self.panel=wx.Panel(self)
        accel_entries = []

        # 模型
        self.model_factory = ModelFactory(language)
        self.current_index = 0
        self.model = None
        self.tags = tags
        # TODO
        self.tag_button_dict = {}
        self.colours = wx.ColourDatabase()
        self.select_colour = self.colours.Find("GREEN")
        self.unselect_colour = self.colours.Find("GREY")

        # 原始文本
        origin_vsizer = wx.BoxSizer(wx.VERTICAL)
    
        origin_hsizer = wx.BoxSizer()
        self.index_text = wx.TextCtrl(self.panel)
        max_page = wx.StaticText(self.panel, label="/{:d}".format(self.model_factory.get_max_index()))
        jump_to = wx.Button(self.panel, label="跳转")
        
        front_page_accel = "a"
        if front_page_accel is not None:
            front_page = wx.Button(self.panel, label="上一条[{:s}]".format("a"))
            accel_entry = wx.AcceleratorEntry(flags=wx.ACCEL_NORMAL, keyCode=ord(front_page_accel), cmd=front_page.GetId())
            accel_entries.append(accel_entry)
        else:
            front_page = wx.Button(self.panel, label="上一条")
        next_page_accel = "d"
        if next_page_accel is not None:
            next_page = wx.Button(self.panel, label="下一条[{:s}]".format(next_page_accel))
            accel_entry = wx.AcceleratorEntry(flags=wx.ACCEL_NORMAL, keyCode=ord(next_page_accel), cmd=next_page.GetId())
            accel_entries.append(accel_entry)
        else:
            next_page = wx.Button(self.panel, label="下一条")

        self.Bind(wx.EVT_BUTTON, lambda event: self.jump_to_page(self.current_index - 1), front_page)
        self.Bind(wx.EVT_BUTTON, lambda event: self.jump_to_page(int(self.index_text.GetValue())), jump_to)
        self.Bind(wx.EVT_BUTTON, lambda event: self.jump_to_page(self.current_index + 1), next_page)

        origin_hsizer.Add(front_page, flag=wx.ALL, border=2)
        origin_hsizer.Add(self.index_text, proportion=1, flag=wx.ALL | wx.EXPAND, border=2)
        origin_hsizer.Add(max_page, flag=wx.ALL, border=2)
        origin_hsizer.Add(jump_to, flag=wx.ALL, border=2)
        origin_hsizer.Add(next_page, flag=wx.ALL, border=2)
        origin_vsizer.Add(origin_hsizer)

        self.title_text = wx.TextCtrl(self.panel, style=wx.TE_READONLY | wx.TE_MULTILINE)
        self.remarks_text = wx.TextCtrl(self.panel, style=wx.TE_READONLY | wx.TE_MULTILINE)
        font = wx.Font(15, 
            wx.MODERN, 
            wx.NORMAL, 
            wx.NORMAL
            )
        self.title_text.SetFont(font)
        self.remarks_text.SetFont(font)
        origin_vsizer.Add(self.title_text, proportion=1, flag=wx.EXPAND | wx.ALL, border=5)
        origin_vsizer.Add(self.remarks_text, proportion=5, flag=wx.EXPAND | wx.ALL, border=5)

        # 分类结果
        category_hsizer = wx.BoxSizer()
        for category, tags in self.tags.items():
            category_vsizer = wx.BoxSizer(wx.VERTICAL)
            category_name = wx.StaticText(self.panel, label=category)
            category_vsizer.Add(category_name, flag=wx.ALL, border=2)
            buttons = [wx.Button(self.panel, label=tag) for tag in tags]
            for i in range(len(tags)):
                self.tag_button_dict[tags[i]] = buttons[i]
                self.Bind(wx.EVT_BUTTON, _EventHandler(self.press_tag_button, tags[i]), buttons[i])
                category_vsizer.Add(buttons[i], proportion=1, flag=wx.EXPAND | wx.ALL, border=5)
            category_hsizer.Add(category_vsizer, proportion=1, flag=wx.EXPAND | wx.ALL, border=5)

        accel_table = wx.AcceleratorTable(accel_entries)

        global_hsizer = wx.BoxSizer()
        global_hsizer.Add(origin_vsizer, proportion=1, flag=wx.EXPAND)
        global_hsizer.Add(category_hsizer, proportion=1, flag=wx.EXPAND)
        self.panel.SetSizer(global_hsizer)
        self.SetAcceleratorTable(accel_table)

    def press_tag_button(self, tag_name: str):
        # print(f"press button {tag_name}")
        if self.model.has_tag(tag_name):
            self.model.delete_tag(tag_name)
            self.tag_button_dict[tag_name].SetBackgroundColour(self.unselect_colour)
        else:
            self.model.add_tag(tag_name)
            self.tag_button_dict[tag_name].SetBackgroundColour(self.select_colour)
        

    def draw_model(self):
        self.index_text.SetValue(str(self.current_index))
        if self.model is None:
            self.title_text.SetValue("")
            self.remarks_text.SetValue("")
            for button in self.tag_button_dict.values:
                button.SetBackgroundColour(self.unselect_colour)
            return
        title = self.model.title
        remarks = self.model.remarks
        self.title_text.SetValue(title if title is not None else "")
        self.remarks_text.SetValue(remarks if remarks is not None else "")
        for name, button in self.tag_button_dict.items():
            if self.model.has_tag(name):
                button.SetBackgroundColour(self.select_colour)
            else:
                button.SetBackgroundColour(self.unselect_colour)
        
    def jump_to_page(self, index: int):
        if index > self.model_factory.get_max_index():
            index = self.model_factory.get_max_index()
        if index < 0:
            index = 0
        if self.model is not None:
            self.model_factory[self.current_index] = self.model
        self.model = self.model_factory[index]
        self.current_index = index
        self.draw_model()



class MyApp(wx.App):
    def OnInit(self):
        super().OnInit()
        self.frame = MyFrame(parent=None, id=-1, language=_checking_language, tags=_checking_tags)
        self.frame.jump_to_page(0)
        self.frame.Show()
        return True

    def OnEventLoopExit(self, loop):
        super().OnEventLoopExit(loop)

_checking_language = None
_checking_tags = None

def start(lang: str, tags: Dict[str, List[str]]):
    global _checking_language
    global _checking_tags
    if _checking_language is not None:
        raise Exception("canot start mark tool")
    try:
        _checking_language = lang
        _checking_tags = tags
        print("start")
        app=MyApp()
        app.MainLoop()
    finally:
        _checking_language = None
        _checking_tags = None


if __name__=='__main__':
    start("C", {"category1": ["C语言", "tag2"], "category2": ["tag3", "tag4", "tag5"]})