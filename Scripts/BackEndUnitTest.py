# -*- coding: utf8 -*-

import os
import sys
import enum
import time
import xml.etree.ElementTree as ET
from utils.Utils import log
from utils.Utils import pcat
from utils.Utils import LogLevel
from utils.Config import Path
from utils.Utils import exec
from utils.Utils import StopException

def restore():
    log("恢复项目依赖")
    command = "dotnet restore {}".format(Path.BackEnd)
    exec(command)

def build_without_restore():
    log("构建项目")
    command = "dotnet build --no-restore {}".format(Path.BackEnd)
    exec(command)

def test_without_build()->str:
    log("运行测试")
    old_dir = None
    if not os.path.exists(Path.AIBotTest_TestResults):
        old_dir = set()
    else:
        old_dir = set(os.listdir(Path.AIBotTest_TestResults))
    command = "dotnet test --no-build --collect:\"XPlat Code Coverage\" {}".format(Path.BackEnd)
    exec(command)
    new_dir = set(os.listdir(Path.AIBotTest_TestResults))
    news = new_dir - old_dir
    assert len(news) == 1
    guid = next(iter(news))
    log("test result guid={}".format(guid))
    return guid

def install_tool_ReportGenerator():
    log("安装覆盖报告生成工具")
    command = "dotnet tool install -g dotnet-reportgenerator-globaltool"
    exec(command, ensure_success=False)

def generate_report(guid: str):
    log("生成覆盖报表")
    result_path = pcat(Path.AIBotTest_TestResults, guid)
    xml_path = pcat(result_path, "coverage.cobertura.xml")
    report_path = pcat(result_path, "coverage_report")
    command = "reportgenerator " +\
            "\"-reports:{}\" ".format(xml_path) +\
            "\"-targetdir:{}\" ".format(report_path) +\
            "-reporttypes:Html"
    exec(command)

def package(guid: str):
    log("打包覆盖报表")
    result_path = pcat(Path.AIBotTest_TestResults, guid)
    xml_path = pcat(result_path, "coverage.cobertura.xml")
    report_path = pcat(result_path, "coverage_report")
    command = "zip -j -r report.zip " + result_path
    exec(command)

def print_cli_message(guid: str):
    result_path = pcat(Path.AIBotTest_TestResults, guid)
    xml_path = pcat(result_path, "coverage.cobertura.xml")
    etree = ET.parse(xml_path)
    root = etree.getroot()
    attr = root.attrib
    line_rate = float(attr["line-rate"])
    branch_rate = float(attr["branch-rate"])
    lines_covered = int(attr["lines-covered"])
    lines_valid = int(attr["lines-valid"])
    branches_valid = int(attr["branches-valid"])
    branches_covered = int(attr["branches-covered"])
    log("test-case-GUID:\t{}".format(guid))
    log("line-coverage:\t{:.2f} % ({:d}/{:d})".format(line_rate * 100, lines_covered, lines_valid))
    log("branch-coverage:\t{:.2f} % ({:d}/{:d})".format(branch_rate * 100, branches_covered, branches_valid))

def open_chrome(guid: str):
    log("使用Chrome浏览器查看报告")
    result_path = pcat(Path.AIBotTest_TestResults, guid)
    xml_path = pcat(result_path, "coverage.cobertura.xml")
    report_path = pcat(result_path, "coverage_report")
    index_path = pcat(report_path, "index.html")
    command = f"start chrome {index_path}"
    exec(command, False)
    
    
def main() -> int:
    try:
        restore()
        build_without_restore()
        guid = test_without_build()
        try:
            install_tool_ReportGenerator()
            generate_report(guid)
            package(guid)
            open_chrome(guid)
            return 0
        finally:
            print_cli_message(guid)
    except StopException as e:
        log(str(e), level=LogLevel.WAR)
        return -1

if __name__ == '__main__':
    ret = main()
    exit(ret)
