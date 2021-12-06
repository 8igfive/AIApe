# -*- coding: utf8 -*-

import enum
import time
import os
import sys
import shutil

class LogLevel(enum.Enum):
    DBG = 0
    INF = 1
    WAR = 2
    ERR = 3

_log_file = None

def enable_log_file(log_file_path : str):
    return _LogFileHandler(log_file_path)

class _LogFileHandler:
    def __init__(self, file_name):
        self.file_name = file_name

    def __enter__(self):
        global _log_file
        if _log_file is not None:
            raise Exception("log file has already set")
        _log_file = open(self.file_name, 'a', encoding='utf8')

    def __exit__(self, exc_type, exc_val, exc_tb):
        global _log_file
        _log_file.close()
        _log_file = None

def get_time_stampe():
    return time.strftime("%Y-%m-%d %H:%M:%S", time.localtime(time.time()))

def log(*msg, level = LogLevel.INF):
    msg = "{}: [{}] {}".format(get_time_stampe(), level.name, " ".join(str(o) for o in msg))
    print(msg)
    global _log_file
    if _log_file is not None:
        _log_file.write(msg + '\n')

def pcat(p: str, c: str) -> str:
    return os.path.abspath(os.path.join(p, c))

class StopException(Exception):
    def __init__(self, *args, **kargs):
        super().__init__(*args, **kargs)

def exec(command: str, ensure_success: bool=True) -> int:
    log("executing command: {}".format(command))
    sys.stdout.flush()
    ret = os.system(command)
    log("executed command with exit-code={}".format(ret))
    if ensure_success and ret != 0:
        raise StopException("exit because command exited with error: {}".format(command))
    return ret


def ensure_and_clean_up_dir(dir_path: str):
    if os.path.exists(dir_path):
        shutil.rmtree(dir_path)
    os.makedirs(dir_path)

def ensure_dir(dir_path: str):
    if not os.path.exists(dir_path):
        os.makedirs(dir_path)