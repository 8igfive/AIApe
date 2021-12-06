import re

class MarkdownRander:
    def __init__(self):
        self.re_rm_blank = re.compile(r'[\n\t\r]*')
        self.re_h = re.compile(r'h(\d)')
        self.remove_none = re.compile(r'```\nNone\n```')
        self.fix_h = re.compile(r'(<h(\d)>(.*?)</h\d>)', re.DOTALL)
        self.rm_div = re.compile(r'<div.*?>|</div>', re.DOTALL)
        self.fix_cb = re.compile(r'(<button.*?data-clipboard-text="(.*?)"[\s\n]*data-original-title.*?>[\s\n]*</button>)', re.DOTALL)
        self.fix_img = re.compile(r'(<img.*?src="(.*?)".*?/>)', re.DOTALL)
        self.sub_src = re.compile(r'src="(.*?)"', re.DOTALL)

    def rander_article(self, article):
        contents = list()
        for content in article.contents:
            if content != '\n':
                if content.name == 'p':
                    contents.append(self._rander_p(content))
                elif content.name == 'pre':
                    contents.append(self._rander_pre(content))
                elif content.name == 'blockquote':
                    contents.append(self._rander_blockquote(content))
                # elif content.name == 'ul':
                #     contents.append(self.rander_ul(content))
                else:
                    contents.append(str(content))
                    # h_match = self.re_h.match(content.name)
                    # if h_match:
                    #     contents.append(self.rander_h(content, int(h_match.group(1))))
        return '\n\n'.join(contents)

    # def rander_ul(self, ul):
    #     pass

    def _rander_p(self, p):
        contents = list()
        for content in p.children:
            if content.name == 'code':
                contents.append('`{}`'.format(content.string))
            elif content.name == 'br':
                contents.append('\n\n')
            else:
                contents.append(self.re_rm_blank.sub('', str(content)))
        return ''.join(contents)
    
    # def rander_h(self, h, l):
    #     contents = ['#'] * l
    #     contents.append(' ')
    #     contents.append(h.string)
    #     return ''.join(contents)

    def _rander_pre(self, pre):
        return '```\n{}\n```'.format(pre.string)
    
    def _rander_blockquote(self, quote):
        contents = list()
        deeper = quote.find_all('p')
        if len(deeper) > 0:
            for content in quote.find_all('p')[0].contents:
                if content.name == 'code':
                    contents.append('`{}`'.format(content.string))
                elif content.name == 'br':
                    contents.append('\n')
                elif content.name == None:
                    contents.append('> {}'.format(self.re_rm_blank.sub('', content)))
        else:
            contents.append('> {}'.format(quote.string))
        return ''.join(contents)

    def fix_markdown(self, origin, base_url='https://segmentfault.com'):
        fixed = self.remove_none.sub('', origin)
        hs = self.fix_h.findall(fixed)
        for h in hs:
            level = int(h[1])
            content = h[2]
            new_h = ['#'] * level
            new_h.append(' ')
            new_h.append(content)
            new_h = ''.join(new_h)
            fixed = fixed.replace(h[0], new_h)
        fixed = self.rm_div.sub('', fixed)
        cbs = self.fix_cb.findall(fixed)
        for cb in cbs:
            new_cb = '```\n{}\n```'.format(cb[1])
            fixed = fixed.replace(cb[0], new_cb)
        imgs = self.fix_img.findall(fixed)
        for img in imgs:
            new_img = self.sub_src.sub('src="{}"'.format(base_url + img[1]), img[0])
            fixed = fixed.replace(img[0], new_img)
        return fixed