using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace Buaa.AIBot.Services.CodeAnalyze
{
    public class CppCheckResultTanslation
    {
        private readonly IReadOnlyDictionary<string, Func<CppCheckResult, string>> full_match_dict;
        private readonly IReadOnlyDictionary<string, Func<CppCheckResult, string>> start_match_dict;
        private readonly ILogger<CppCheckResultTanslation> logger;

        public CppCheckResultTanslation(ILogger<CppCheckResultTanslation> logger)
        {
            this.logger = logger;
            var tmp = new Dictionary<string, Func<CppCheckResult, string>>()
            {
                ["AssignmentIntegerToAddress"] = AssignmentIntegerToAddress,
                ["CastAddressToIntegerAtReturn"] = CastAddressToIntegerAtReturn,
                ["arithOperationsOnVoidPointer"] = ArithOperationsOnVoidPointer,
                ["arrayIndexOutOfBounds"] = ArrayIndexOutOfBounds,
                ["assertWithSideEffect"] = AssertWithSideEffect,
                ["danglingLifetime"] = DanglingLifetime,
                ["deallocret"] = Deallocret,
                ["memleak"] = MemLeak,
                ["resourceLeak"] = ResourceLeak,
                ["unassignedVariable"] = UnassignedVariable,
                ["uninitdata"] = Uninitdata,
                ["uninitvar"] = Uninitvar,
                ["deallocuse"] = Deallocuse,
                ["unreadVariable"] = UnreadVariable,
                ["unsignedLessThanZero"] = UnsignedLessThanZero,
                ["unusedAllocatedMemory"] = UnusedAllocatedMemory,
                ["unusedFunction"] = UnusedFunction,
                ["unusedStructMember"] = UnusedStructMember,
                ["unusedVariable"] = UnusedVariable,
                ["variableScope"] = VariableScope,
                ["wrongPrintfScanfArgNum"] = WrongPrintfScanfArgNum,
            };

            full_match_dict = new Dictionary<string, Func<CppCheckResult, string>>(
                tmp.Select(kv => 
                    new KeyValuePair<string, Func<CppCheckResult, string>>(kv.Key.ToLowerInvariant(), kv.Value)));

            tmp = new Dictionary<string, Func<CppCheckResult, string>>()
            {
                ["invalidScanfArgType"] = InvalidScanfArgType,
                ["invalidPrintfArgType"] = InvalidPrintfArgType
            };

            start_match_dict = new Dictionary<string, Func<CppCheckResult, string>>(
                tmp.Select(kv =>
                    new KeyValuePair<string, Func<CppCheckResult, string>>(kv.Key.ToLowerInvariant(), kv.Value)));
        }

        public string Translate(CppCheckResult cppCheckResult)
        {
            var lowerCategory = cppCheckResult.Category.ToLowerInvariant();
            if (full_match_dict.TryGetValue(lowerCategory, out var func))
            {
                return func(cppCheckResult);
            }
            foreach (var pair in start_match_dict)
            {
                if (lowerCategory.StartsWith(pair.Key))
                {
                    return pair.Value(cppCheckResult);
                }
            }
            this.logger.LogError("Unknown category for CppCheck result {cppCheckResult}", cppCheckResult);
            return $"暂无翻译的错误信息：[{cppCheckResult.Category}] {cppCheckResult.Message}";
        }

        private string AssignmentIntegerToAddress(CppCheckResult cppCheckResult)
        {
            return "将一个指针赋值给一个整型，将进行可能导致错误的指针向整型的转型";
        }

        private string CastAddressToIntegerAtReturn(CppCheckResult cppCheckResult)
        {
            return "在返回值为整型的函数中，返回了指针类型，将进行可能导致错误的指针向整型的转型";
        }

        private string ArithOperationsOnVoidPointer(CppCheckResult cppCheckResult)
        {
            string name = cppCheckResult.Symbol;
            var mc = Regex.Match(cppCheckResult.Message, "'(?<name>[\\w\\d_\\(\\)\\:\\.\\*]+)' is of type '(?<type>[\\w\\d_\\*]+)'\\. When using void pointers in calculations, the behaviour is undefined");
            
            if (string.IsNullOrEmpty(name))
            {
                name = mc.Groups["name"].ToString();
            }
            string type = mc.Groups["type"].ToString();
            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(type))
            {
                return $"'{name}'的类型为'{type}'，对一个'void*'指针进行计算是未定义行为";
            }
            if(!string.IsNullOrEmpty(name))
            {
                return $"对一个类型为'void*'的指针'{name}'进行计算是未定义行为";
            }
            return "对一个类型为'void*'的指针进行计算是未定义行为";
        }

        private string ArrayIndexOutOfBounds(CppCheckResult cppCheckResult)
        {
            var mc = Regex.Match(cppCheckResult.Message, "Array '(?<name>[\\d\\w\\:_]+)' accessed at index '(?<index>[\\d\\w]+)', which is out of bounds");
            var name = mc.Groups["name"].ToString();
            var index = mc.Groups["index"].ToString();
            return $"数组'{name}'访问了下标为{index}的元素，该下标越界";
        }

        private string AssertWithSideEffect(CppCheckResult cppCheckResult)
        {
            string name = cppCheckResult.Symbol;
            if (string.IsNullOrEmpty(name))
            {
                var mc = Regex.Match(cppCheckResult.Message, "Assert statement calls a function which may have desired side effects: '(?<index>[\\d\\w]+)'");
                name = mc.Groups["name"].ToString();
            }
            if (!string.IsNullOrEmpty(name))
            {
                return $"断言语句调用了可能产生副作用的函数：{name}";
            }
            return $"断言语句可能产生副作用";
        }

        private string DanglingLifetime(CppCheckResult cppCheckResult)
        {
            string name = cppCheckResult.Symbol;
            if (string.IsNullOrEmpty(name))
            {
                var mc = Regex.Match(cppCheckResult.Message, "The scope of the variable '(?<name>[\\d\\w\\:_]+)' can be reduced");
                name = mc.Groups["name"].ToString();
            }
            return $"悬垂指针'{name}'，非局部变量指针指向了局部变量";
        }

        private string Deallocret(CppCheckResult cppCheckResult)
        {
            string name = cppCheckResult.Symbol;
            if (string.IsNullOrEmpty(name))
            {
                var mc = Regex.Match(cppCheckResult.Message, "Returning/dereferencing '(?<name>[\\d\\w\\:_]+)' after it is deallocated / released");
                name = mc.Groups["name"].ToString();
            }
            return $"尝试返回或者访问一个已失效的指针'{name}'，因为其指向的动态分配的内存已经被释放";
        }

        private string Deallocuse(CppCheckResult cppCheckResult)
        {
            string name = cppCheckResult.Symbol;
            if (string.IsNullOrEmpty(name))
            {
                var mc = Regex.Match(cppCheckResult.Message, "Dereferencing '(?<name>[\\d\\w\\:_]+)' after it is deallocated / released");
                name = mc.Groups["name"].ToString();
            }
            return $"尝试访问一个已失效的指针'{name}'，因为其指向的动态分配的内存已经被释放";
        }

        private string MemLeak(CppCheckResult cppCheckResult)
        {
            string name = cppCheckResult.Symbol;
            if (string.IsNullOrEmpty(name))
            {
                var mc = Regex.Match(cppCheckResult.Message, "Memory leak: (?<name>[\\d\\w\\:_]+)");
                name = mc.Groups["name"].ToString();
            }
            return $"内存泄漏：'{name}'";
        }

        private string ResourceLeak(CppCheckResult cppCheckResult)
        {
            string name = cppCheckResult.Symbol;
            if (string.IsNullOrEmpty(name))
            {
                var mc = Regex.Match(cppCheckResult.Message, "Resource leak: (?<name>[\\d\\w\\:_]+)");
                name = mc.Groups["name"].ToString();
            }
            return $"资源泄露：'{name}'";
        }

        private string UnassignedVariable(CppCheckResult cppCheckResult)
        {
            string name = cppCheckResult.Symbol;
            if (string.IsNullOrEmpty(name))
            {
                var mc = Regex.Match(cppCheckResult.Message, "Variable '(?<name>[\\d\\w\\:_]+)' is not assigned a value");
                name = mc.Groups["name"].ToString();
            }
            return $"变量'{name}'没有进行赋值";
        }

        private string Uninitdata(CppCheckResult cppCheckResult)
        {
            string name = cppCheckResult.Symbol;
            if (string.IsNullOrEmpty(name))
            {
                var mc = Regex.Match(cppCheckResult.Message, "Memory is allocated but not initialized: (?<name>[\\d\\w\\:_]+)");
                name = mc.Groups["name"].ToString();
            }
            return $"访问了动态申请的内存，但是其尚未进行初始化：'{name}'";
        }

        private string Uninitvar(CppCheckResult cppCheckResult)
        {
            string name = cppCheckResult.Symbol;
            if (string.IsNullOrEmpty(name))
            {
                var mc = Regex.Match(cppCheckResult.Message, "Uninitialized variable: (?<name>[\\d\\w\\:_]+)");
                name = mc.Groups["name"].ToString();
            }
            return $"使用未初始化的变量：'{name}'";
        }

        private string UnreadVariable(CppCheckResult cppCheckResult)
        {
            string name = cppCheckResult.Symbol;
            if (string.IsNullOrEmpty(name))
            {
                var mc = Regex.Match(cppCheckResult.Message, "Variable '(?<name>[\\d\\w\\:_]+)' is assigned a value that is never used");
                name = mc.Groups["name"].ToString();
            }
            return $"变量初始化后没有使用：'{name}'";
        }

        private string UnsignedLessThanZero(CppCheckResult cppCheckResult)
        {
            string name = cppCheckResult.Symbol;
            if (string.IsNullOrEmpty(name))
            {
                var mc = Regex.Match(cppCheckResult.Message, "Checking if unsigned expression '(?<name>[\\d\\w\\:_\\(\\)\\/\\+\\-=\\&\\,\\.\\:\\?]+)' is less than zero.");
                name = mc.Groups["name"].ToString();
            }
            return $"尝试比较无符号表达式（总为非负的表达式）'{name}'小于零";
        }

        private string UnusedAllocatedMemory(CppCheckResult cppCheckResult)
        {
            string name = cppCheckResult.Symbol;
            if (string.IsNullOrEmpty(name))
            {
                var mc = Regex.Match(cppCheckResult.Message, "Variable '(?<name>[\\d\\w\\:_]+)' is allocated memory that is never used");
                name = mc.Groups["name"].ToString();
            }
            return $"申请了动态内存但没有使用：'{name}'";
        }

        private string UnusedFunction(CppCheckResult cppCheckResult)
        {
            string name = cppCheckResult.Symbol;
            if (string.IsNullOrEmpty(name))
            {
                var mc = Regex.Match(cppCheckResult.Message, "The function '(?<name>[\\d\\w\\:_]+)' is never used");
                name = mc.Groups["name"].ToString();
            }
            return $"从未使用过的函数：'{name}'";
        }

        private string UnusedStructMember(CppCheckResult cppCheckResult)
        {
            string name = cppCheckResult.Symbol;
            if (string.IsNullOrEmpty(name))
            {
                var mc = Regex.Match(cppCheckResult.Message, "struct member '(?<name>[\\d\\w\\:_]+)' is never used.");
                name = mc.Groups["name"].ToString();
            }
            return $"从未使用过的结构体成员变量：'{name}'";
        }

        private string UnusedVariable(CppCheckResult cppCheckResult)
        {
            string name = cppCheckResult.Symbol;
            if (string.IsNullOrEmpty(name))
            {
                var mc = Regex.Match(cppCheckResult.Message, "Unused variable: (?<name>[\\d\\w\\:_]+)");
                name = mc.Groups["name"].ToString();
            }
            return $"从未使用过的变量：'{name}'";
        }

        private string VariableScope(CppCheckResult cppCheckResult)
        {
            string name = cppCheckResult.Symbol;
            if (string.IsNullOrEmpty(name))
            {
                var mc = Regex.Match(cppCheckResult.Message, "The scope of the variable '(?<name>[\\d\\w\\:_]+)' can be reduced");
                name = mc.Groups["name"].ToString();
            }
            return $"变量'{name}'的作用域可以缩小";
        }

        private string WrongPrintfScanfArgNum(CppCheckResult cppCheckResult)
        {
            var mc = Regex.Match(cppCheckResult.Message, "(?<func>[\\w\\d_]+) format string requires (?<expect>\\d+) parameter but only (?<actual>\\d+) are given.");
            string func = mc.Groups["func"].ToString();
            string expect = mc.Groups["expect"].ToString();
            string actual = mc.Groups["actual"].ToString();
            return $"{func}的参数数量错误，需要{expect}个参数，但是只传入了{actual}个";
        }

        private string InvalidPrintfArgType(CppCheckResult cppCheckResult)
        {
            var mc = Regex.Match(cppCheckResult.Message,
                "(?<fmt>[\\w\\d\\%\\.]+) in format string \\(no\\. (?<index>\\d+)\\) requires '(?<expect>[\\d\\w\\{\\}\\(\\)\\[\\]\\*\\._]+)' but the argument type is '(?<actual>[\\d\\w\\{\\}\\(\\)\\[\\]\\*\\._]+)'\\.");
            var fmt = mc.Groups["fmt"].ToString();
            var index = mc.Groups["index"].ToString();
            var expect = mc.Groups["expect"].ToString();
            var actual = mc.Groups["actual"].ToString();
            return $"不匹配的printf参数：格式描述中'{fmt}'（NO.{index}）要求的数据类型为'{expect}'，但是传入的参数的类型为'{actual}'";
        }

        private string InvalidScanfArgType(CppCheckResult cppCheckResult)
        {
            var mc = Regex.Match(cppCheckResult.Message,
                "(?<fmt>[\\w\\d\\%\\.]+) in format string \\(no\\. (?<index>\\d+)\\) requires '(?<expect>[\\d\\w\\{\\}\\(\\)\\[\\]\\*\\._]+)' but the argument type is '(?<actual>[\\d\\w\\{\\}\\(\\)\\[\\]\\*\\._]+)'\\.");
            var fmt = mc.Groups["fmt"].ToString();
            var index = mc.Groups["index"].ToString();
            var expect = mc.Groups["expect"].ToString();
            var actual = mc.Groups["actual"].ToString();
            return $"不匹配的scanf参数：格式描述中'{fmt}'（NO.{index}）要求的数据类型为'{expect}'，但是传入的参数的类型为'{actual}'";
        }
    }
}
