using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using MySimpleJSON;

public class SlowStringStorableExample : MVRScript
{
    public override void Init()
    {
        CreateButton("Simulate Save").button.onClick.AddListener(() =>
        {
            var sw = Stopwatch.StartNew();
            var jc = new JSONClass();
            jc.SetThis("test", new string('X', 50000));
            var sb = new StringBuilder();
            jc.ToString(string.Empty, sb);
            SuperController.LogMessage($"Simulation: {sw.Elapsed.TotalSeconds:0.000}s");
        });
    }
}

namespace MySimpleJSON
{
    public class JSONNode
    {
        public virtual JSONNode GetThis(string aKey)
        {
            return null;
        }

        public virtual void SetThis(string aKey, JSONNode value)
        {
        }

        public virtual string Value
        {
            get
            {
                return string.Empty;
            }
            set
            {
            }
        }

        public virtual int Count
        {
            get
            {
                return 0;
            }
        }

        public virtual IEnumerable<JSONNode> Childs
        {
            get
            {
                yield break;
            }
        }

        public IEnumerable<JSONNode> DeepChilds
        {
            get
            {
                foreach (JSONNode C in Childs)
                {
                    foreach (JSONNode deepChild in C.DeepChilds)
                    {
                        yield return deepChild;
                    }
                }
            }
        }

        public virtual int AsInt
        {
            get
            {
                int result = 0;
                if (int.TryParse(Value, out result))
                {
                    return result;
                }
                return 0;
            }
            set
            {
                Value = value.ToString();
            }
        }

        public virtual float AsFloat
        {
            get
            {
                float result = 0f;
                if (float.TryParse(Value, out result))
                {
                    return result;
                }
                return 0f;
            }
            set
            {
                Value = value.ToString();
            }
        }

        public virtual double AsDouble
        {
            get
            {
                double result = 0.0;
                if (double.TryParse(Value, out result))
                {
                    return result;
                }
                return 0.0;
            }
            set
            {
                Value = value.ToString();
            }
        }

        public virtual bool AsBool
        {
            get
            {
                bool result = false;
                if (bool.TryParse(Value, out result))
                {
                    return result;
                }
                return !string.IsNullOrEmpty(Value);
            }
            set
            {
                Value = ((!value) ? "false" : "true");
            }
        }

        public virtual JSONClass AsObject
        {
            get
            {
                return this as JSONClass;
            }
        }

        public virtual void Add(string aKey, JSONNode aItem)
        {
        }

        public virtual void Add(JSONNode aItem)
        {
            Add(string.Empty, aItem);
        }

        public virtual JSONNode Remove(string aKey)
        {
            return null;
        }

        public virtual JSONNode Remove(int aIndex)
        {
            return null;
        }

        public virtual JSONNode Remove(JSONNode aNode)
        {
            return aNode;
        }

        public override string ToString()
        {
            return "JSONNode";
        }

        public virtual string ToString(string aPrefix)
        {
            return "JSONNode";
        }

        public virtual void ToString(string aPrefix, StringBuilder sb)
        {
        }

        public static implicit operator JSONNode(string s)
        {
            return new JSONData(s);
        }

        public static implicit operator string(JSONNode d)
        {
            return (!(d == null)) ? d.Value : null;
        }

        public static bool operator ==(JSONNode a, object b)
        {
            if (b == null && a is JSONLazyCreator)
            {
                return true;
            }
            return object.ReferenceEquals(a, b);
        }

        public static bool operator !=(JSONNode a, object b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return object.ReferenceEquals(this, obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        internal static string Escape(string aText)
        {
            string text = string.Empty;
            foreach (char c in aText)
            {
                switch (c)
                {
                    case '\\':
                        text += "\\\\";
                        break;
                    case '"':
                        text += "\\\"";
                        break;
                    case '\n':
                        text += "\\n";
                        break;
                    case '\r':
                        text += "\\r";
                        break;
                    case '\t':
                        text += "\\t";
                        break;
                    case '\b':
                        text += "\\b";
                        break;
                    case '\f':
                        text += "\\f";
                        break;
                    default:
                        text += c;
                        break;
                }
            }
            return text;
        }

        public static JSONNode Parse(string aJSON)
        {
            Stack<JSONNode> stack = new Stack<JSONNode>();
            JSONNode jSONNode = null;
            int i = 0;
            bool flag = false;
            string text = string.Empty;
            string text2 = string.Empty;
            bool flag2 = false;
            for (; i < aJSON.Length; i++)
            {
                switch (aJSON[i])
                {
                    case '{':
                        if (flag2)
                        {
                            text += aJSON[i];
                            break;
                        }
                        stack.Push(new JSONClass());
                        if (jSONNode != null)
                        {
                            text2 = text2.Trim();
                            if (text2 != string.Empty)
                            {
                                jSONNode.Add(text2, stack.Peek());
                            }
                        }
                        text2 = string.Empty;
                        text = string.Empty;
                        flag = false;
                        jSONNode = stack.Peek();
                        break;
                    case '[':
                        throw new NotImplementedException();
                    case ']':
                    case '}':
                        if (flag2)
                        {
                            text += aJSON[i];
                            break;
                        }
                        if (stack.Count == 0)
                        {
                            throw new Exception("JSON Parse: Too many closing brackets");
                        }
                        stack.Pop();
                        if (flag)
                        {
                            text2 = text2.Trim();
                            if (text2 != string.Empty)
                            {
                                jSONNode.Add(text2, text);
                            }
                        }
                        text2 = string.Empty;
                        text = string.Empty;
                        flag = false;
                        if (stack.Count > 0)
                        {
                            jSONNode = stack.Peek();
                        }
                        break;
                    case ':':
                        if (flag2)
                        {
                            text += aJSON[i];
                            break;
                        }
                        text2 = text;
                        text = string.Empty;
                        flag = false;
                        break;
                    case '"':
                        flag2 = ((byte)((flag2 ? 1 : 0) ^ 1) != 0);
                        flag = true;
                        break;
                    case ',':
                        if (flag2)
                        {
                            text += aJSON[i];
                            break;
                        }
                        if (flag)
                        {
                            if (text2 != string.Empty)
                            {
                                jSONNode.Add(text2, text);
                            }
                        }
                        text2 = string.Empty;
                        text = string.Empty;
                        flag = false;
                        break;
                    case '\t':
                    case ' ':
                        if (flag2)
                        {
                            text += aJSON[i];
                        }
                        break;
                    case '\\':
                        i++;
                        if (flag2)
                        {
                            char c = aJSON[i];
                            switch (c)
                            {
                                case 't':
                                    text += '\t';
                                    break;
                                case 'r':
                                    text += '\r';
                                    break;
                                case 'n':
                                    text += '\n';
                                    break;
                                case 'b':
                                    text += '\b';
                                    break;
                                case 'f':
                                    text += '\f';
                                    break;
                                case 'u':
                                    {
                                        string s = aJSON.Substring(i + 1, 4);
                                        text += (char)int.Parse(s, NumberStyles.AllowHexSpecifier);
                                        i += 4;
                                        break;
                                    }
                                default:
                                    text += c;
                                    break;
                            }
                        }
                        break;
                    default:
                        text += aJSON[i];
                        flag = true;
                        break;
                    case '\n':
                    case '\r':
                        break;
                }
            }
            if (flag2)
            {
                throw new Exception("JSON Parse: Quotation marks seems to be messed up.");
            }
            return jSONNode;
        }
    }

    public class JSONClass : JSONNode, IEnumerable
    {
        private Dictionary<string, JSONNode> m_Dict = new Dictionary<string, JSONNode>();

        public override JSONNode GetThis(string aKey)
        {
                if (m_Dict.ContainsKey(aKey))
                {
                    return m_Dict[aKey];
                }
                return new JSONLazyCreator(this, aKey);
        }

        public override void SetThis(string aKey, JSONNode value)
        {
                if (m_Dict.ContainsKey(aKey))
                {
                    m_Dict[aKey] = value;
                }
                else
                {
                    m_Dict.Add(aKey, value);
                }
        }

        public override int Count
        {
            get
            {
                return m_Dict.Count;
            }
        }

        public IEnumerable<string> Keys
        {
            get
            {
                foreach (KeyValuePair<string, JSONNode> item in m_Dict)
                {
                    yield return item.Key;
                }
            }
        }

        public override IEnumerable<JSONNode> Childs
        {
            get
            {
                foreach (KeyValuePair<string, JSONNode> item in m_Dict)
                {
                    yield return item.Value;
                }
            }
        }

        public bool HasKey(string aKey)
        {
            return m_Dict.ContainsKey(aKey);
        }

        public override void Add(string aKey, JSONNode aItem)
        {
            if (!string.IsNullOrEmpty(aKey))
            {
                if (m_Dict.ContainsKey(aKey))
                {
                    m_Dict[aKey] = aItem;
                }
                else
                {
                    m_Dict.Add(aKey, aItem);
                }
            }
            else
            {
                m_Dict.Add(Guid.NewGuid().ToString(), aItem);
            }
        }

        public override JSONNode Remove(string aKey)
        {
            if (!m_Dict.ContainsKey(aKey))
            {
                return null;
            }
            JSONNode result = m_Dict[aKey];
            m_Dict.Remove(aKey);
            return result;
        }

        public override JSONNode Remove(int aIndex)
        {
            if (aIndex < 0 || aIndex >= m_Dict.Count)
            {
                return null;
            }
            KeyValuePair<string, JSONNode> keyValuePair = m_Dict.ElementAt(aIndex);
            m_Dict.Remove(keyValuePair.Key);
            return keyValuePair.Value;
        }

        public override JSONNode Remove(JSONNode aNode)
        {
            try
            {
                KeyValuePair<string, JSONNode> keyValuePair = (from k in m_Dict
                                                               where k.Value == aNode
                                                               select k).First();
                m_Dict.Remove(keyValuePair.Key);
                return aNode;
            }
            catch
            {
                return null;
            }
        }

        public IEnumerator GetEnumerator()
        {
            foreach (KeyValuePair<string, JSONNode> N in m_Dict)
            {
                yield return N;
            }
        }

        public override string ToString()
        {
            string text = "{";
            foreach (KeyValuePair<string, JSONNode> item in m_Dict)
            {
                if (text.Length > 2)
                {
                    text += ", ";
                }
                string text2 = text;
                text = text2 + "\"" + JSONNode.Escape(item.Key) + "\":" + item.Value.ToString();
            }
            return text + "}";
        }

        public override string ToString(string aPrefix)
        {
            string text = "{ ";
            foreach (KeyValuePair<string, JSONNode> item in m_Dict)
            {
                if (text.Length > 3)
                {
                    text += ", ";
                }
                text = text + "\n" + aPrefix + "   ";
                string text2 = text;
                text = text2 + "\"" + JSONNode.Escape(item.Key) + "\" : " + item.Value.ToString(aPrefix + "   ");
            }
            return text + "\n" + aPrefix + "}";
        }

        public override void ToString(string aPrefix, StringBuilder sb)
        {
            bool flag = true;
            sb.Append("{ ");
            foreach (KeyValuePair<string, JSONNode> item in m_Dict)
            {
                if (!flag)
                {
                    sb.Append(", ");
                }
                flag = false;
                sb.Append("\n" + aPrefix + "   ");
                sb.Append("\"" + JSONNode.Escape(item.Key) + "\" : ");
                item.Value.ToString(aPrefix + "   ", sb);
            }
            sb.Append("\n" + aPrefix + "}");
        }
    }
    internal class JSONLazyCreator : JSONNode
    {
        private JSONNode m_Node;

        private string m_Key;

        public override JSONNode GetThis(string aKey)
        {
                return new JSONLazyCreator(this, aKey);
        }

        public override void SetThis(string aKey, JSONNode value)
        {
                JSONClass jSONClass = new JSONClass();
                jSONClass.Add(aKey, value);
                Set(jSONClass);
        }

        public override JSONClass AsObject
        {
            get
            {
                JSONClass jSONClass = new JSONClass();
                Set(jSONClass);
                return jSONClass;
            }
        }

        public JSONLazyCreator(JSONNode aNode)
        {
            m_Node = aNode;
            m_Key = null;
        }

        public JSONLazyCreator(JSONNode aNode, string aKey)
        {
            m_Node = aNode;
            m_Key = aKey;
        }

        private void Set(JSONNode aVal)
        {
            if (m_Key == null)
            {
                m_Node.Add(aVal);
            }
            else
            {
                m_Node.Add(m_Key, aVal);
            }
            m_Node = null;
        }

        public override void Add(string aKey, JSONNode aItem)
        {
            JSONClass jSONClass = new JSONClass();
            jSONClass.Add(aKey, aItem);
            Set(jSONClass);
        }

        public static bool operator ==(JSONLazyCreator a, object b)
        {
            if (b == null)
            {
                return true;
            }
            return object.ReferenceEquals(a, b);
        }

        public static bool operator !=(JSONLazyCreator a, object b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return true;
            }
            return object.ReferenceEquals(this, obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return string.Empty;
        }

        public override string ToString(string aPrefix)
        {
            return string.Empty;
        }

        public override void ToString(string aPrefix, StringBuilder sb)
        {
        }
    }
    public class JSONData : JSONNode
    {
        private string m_Data;

        public override string Value
        {
            get
            {
                return m_Data;
            }
            set
            {
                m_Data = value;
            }
        }

        public JSONData(string aData)
        {
            m_Data = aData;
        }

        public JSONData(float aData)
        {
            AsFloat = aData;
        }

        public JSONData(double aData)
        {
            AsDouble = aData;
        }

        public JSONData(bool aData)
        {
            AsBool = aData;
        }

        public JSONData(int aData)
        {
            AsInt = aData;
        }

        public override string ToString()
        {
            return "\"" + JSONNode.Escape(m_Data) + "\"";
        }

        public override string ToString(string aPrefix)
        {
            return "\"" + JSONNode.Escape(m_Data) + "\"";
        }

        public override void ToString(string aPrefix, StringBuilder sb)
        {
            sb.Append("\"" + JSONNode.Escape(m_Data) + "\"");
        }
    }
}

