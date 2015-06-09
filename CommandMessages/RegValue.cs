// Copyright 2015 Steve Meckl
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Text.RegularExpressions;
using System.Runtime.Serialization;
using qleqtr.Shared;

namespace qleqtr.CommandMessages
{
    public enum RegValueType
    {
        String,
        Binary,
        DWord,
        ExpandString,
        MultiString,
        QWord,
        Unknown,
        None
    };

    [DataContract]
    [KnownType(typeof(RegStringValue))]
    [KnownType(typeof(RegExpandStringValue))]
    [KnownType(typeof(RegMultiStringValue))]
    [KnownType(typeof(RegBinaryValue))]
    [KnownType(typeof(RegDWORDValue))]
    [KnownType(typeof(RegQWORDValue))]
    public abstract class RegValue
    {
        [DataMember]
        public RegValueType type { get; set; }

        [DataMember]
        public String name { get; set; }
        
        public RegValue(RegValueType valType, String valName)
        {
            type = valType;
            name = valName;
        }

        // In general, a RegValue is valid if the name consists of printable characters
        // other than the backslash
        public virtual bool isValid()
        {
            bool bRet = true;

            Regex valueRegEx = new Regex(CollectionAgentMessage.RegexPrintableCharsMinusBackslash, RegexOptions.IgnoreCase);

            if (!valueRegEx.IsMatch(name))
                bRet = false;

            return bRet;
        }
    }

    [DataContract]
    public class RegStringValue : RegValue
    {
        public RegStringValue(String valName, String strVal) : base(RegValueType.String, valName)
        {
            value = strVal;
        }

        [DataMember]
        public String value { get; set; }

        public override bool isValid()
        {
            bool bRet = true;

            if (!base.isValid())
                bRet = false;

            if(bRet)
            {
                Regex valueRegEx = new Regex(CollectionAgentMessage.RegexPrintableChars, RegexOptions.IgnoreCase);

                if (!valueRegEx.IsMatch(name))
                    bRet = false;
            }

            return bRet;
        }
    }

    [DataContract]
    public class RegExpandStringValue : RegValue
    {
        public RegExpandStringValue(String valName, String strVal, String strExpandVal) : base(RegValueType.ExpandString, valName)
        {
            value = strVal;
            expandValue = strExpandVal;
        }

        [DataMember]
        public String value { get; set; }

        [DataMember]
        public String expandValue { get; set; }

        // This class is valid if the value and expand valuse are comprised of
        // printable characters except the backslash.
        public override bool isValid()
        {
            bool bRet = true;

            // Validate the base class
            if (!base.isValid())
                bRet = false;

            // validate the value attribute
            if (bRet)
            {
                Regex valueRegEx = new Regex(CollectionAgentMessage.RegexPrintableChars, RegexOptions.IgnoreCase);

                if (!valueRegEx.IsMatch(value))
                    bRet = false;
            }

            // Validate the expanded string
            if (bRet)
            {
                Regex valueRegEx = new Regex(CollectionAgentMessage.RegexPrintableChars, RegexOptions.IgnoreCase);

                if (!valueRegEx.IsMatch(expandValue))
                    bRet = false;
            }

            return bRet;
        }
    }

    [DataContract]
    public class RegMultiStringValue : RegValue
    {
        public RegMultiStringValue(String valName, String[] strValues) : base(RegValueType.MultiString, valName)
        {
            values = strValues;
        }

        [DataMember]
        public String[] values { get; set; }

        // A MultiString is valid if all strings in the array are comprised
        // of printable chracters.
        public override bool isValid()
        {
            bool bRet = true;

            if (!base.isValid())
                bRet = false;

            if(!bRet)
            {
                Regex valueRegEx = new Regex(CollectionAgentMessage.RegexPrintableChars, RegexOptions.IgnoreCase);

                for(int i = 0; bRet && i < values.Length; i++)
                {
                    if (!valueRegEx.IsMatch(values[i]))
                        bRet = false;
                }
            }

            return bRet;
        }
    }

    [DataContract]
    public class RegBinaryValue : RegValue
    {
        public RegBinaryValue(String valName, byte[] binVal) : base(RegValueType.Binary, valName)
        {
            value = binVal;
        }

        [DataMember]
        public byte[] value { get; set; }
    }

    [DataContract]
    public class RegDWORDValue : RegValue
    {
        public RegDWORDValue(String valName, int dwordVal) : base(RegValueType.DWord, valName)
        {
            value = dwordVal;
        }

        [DataMember]
        public int value { get; set; }
    }

    [DataContract]
    public class RegQWORDValue : RegValue
    {
        public RegQWORDValue(String valName, long qwordVal) : base(RegValueType.QWord, valName)
        {
            value = qwordVal;
        }

        [DataMember]
        public long value { get; set; }
    }
}
