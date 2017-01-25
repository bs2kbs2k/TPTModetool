using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace modEdit
{
    class CodeManager
    {
        string code = "";

        Dictionary<string, string> defaults;

        public CodeManager() {
            defaults = new Dictionary<string, string>();

            defaults.Add("Temperature", "295.15");
            defaults.Add("HotAir", "0");
            defaults.Add("Flammable", "0");
            defaults.Add("Hardness", "0");
            defaults.Add("HeatConduct", "100");

            defaults.Add("LowTemperature", "N/A");
            defaults.Add("LowTemperatureTransition", "");
            defaults.Add("HighTemperature", "N/A");
            defaults.Add("HighTemperatureTransition", "");

            defaults.Add("LowPressure", "N/A");
            defaults.Add("LowPressureTransition", "");
            defaults.Add("HighPressure", "N/A");
            defaults.Add("HighPressureTransition", "");
        }
        public void setCode(string code) {
            this.code = code;
        }
        public string getCode() { return code; }

        public List<string> getCodeNameList() {
            //물질 이름이 들어갈 리스트
            List<string> nameList = new List<string>();

            //물질들을 가져온다.
            foreach (Match elementMatch in Regex.Matches(code, "local ([A-Za-z0-9]{1,4})\\s*=\\s*elements.allocate\\(\"[A-Za-z_]+\",\\s*\"([A-Za-z0-9]{1,4})\"\\)")) {
                // 물질 이름을 가져온다.
                string elementName = elementMatch.ToString().Split(' ')[1];
                nameList.Add(elementName);
            }

            return nameList;
        }

        //물질 프롭 불러오기
        public string getElementProp(string name, string prop) {
            if (getCodeType(name) == "Element") {
                if (prop == "Type") {
                    //타입을 불러온다
                    string elementType = "";
                    string elementBase = Regex.Match(code, "elements.element\\(" + name + ",\\s*elements.element\\(elements\\.DEFAULT_PT_[A-Za-z0-9]{1,4}\\)\\)").ToString();
                    elementBase = Regex.Match(elementBase, "(BOYL|DMND|SOAP|BRMT)").ToString();
                    switch (elementBase){
                        case "BOYL":
                            elementType = "GAS"; break;
                        case "DMND":
                            elementType = "SOLID"; break;
                        case "SOAP":
                            elementType = "LIQUID"; break;
                        case "BRMT":
                            elementType = "POWDER"; break;
                    }
                    return elementType;
                } else {
                    //프롭을 불러온다
                    string line = Regex.Match(code, "elements\\.property\\(" + name + ",\\s*\"" + prop + "\",\\s*[ -~]+\\)").ToString();
                    if (line == "") { return defaults[prop]; }
                    //프롭값을 뽑아온다.
                    line = Regex.Replace(line, "^elements\\.property\\(" + name + ",", "");
                    line = Regex.Replace(line, "\\)\\s*$", "");
                    line = Regex.Replace(line, "\\s+$", "");
                    line = Regex.Replace(line, "\"" + prop + "\",", "");
                    line = Regex.Replace(line, "^\\s+", "");

                    string value = line;
                    return value;
                }
            }

            //뭣도 아니면 null반환
            return null;
        }
        //물질 프롭 설정하기
        public void setElementProp(string name, string prop, string value){
            if (getCodeType(name) == "Element") {
                if (prop == "Type") {
                    // 1. element 함수 교체
                    // element 함수를 찾음
                    string elementMatch = Regex.Match(code, "elements\\.element\\(" + name + ",\\s*elements\\.element\\(elements\\.DEFAULT_PT_[A-Za-z0-9]{1,4}\\)\\)").ToString();
                    // 물질 기반을 정함
                    string elementBase = "";
                    switch (value){
                        case "GAS":
                            elementBase = "BOYL"; break;
                        case "SOLID":
                            elementBase = "DMND"; break;
                        case "LIQUID":
                            elementBase = "SOAP"; break;
                        case "POWDER":
                            elementBase = "BRMT"; break;
                    }
                    //element 함수 교체
                    code = code.Replace(elementMatch, "elements.element(" + name + ", elements.element(elements.DEFAULT_PT_" + elementBase + "))");
                }
                else {
                    string propertiesMatch = Regex.Match(code, "elements\\.property\\(" + name + ",\\s*\"" + prop + "\",\\s*[ -~]+\\)").ToString();
                    if (propertiesMatch == "" && value != null) {
                        // 없으면 생성
                        // 마지막 property를 찾는다.
                        MatchCollection matches = Regex.Matches(code, "elements\\.property\\(" + name + ",\\s*\"[A-Za-z0-9]+\",\\s*[ -~]+\\)");
                        string lastProp = matches[matches.Count-1].ToString();
                        // 그리고 넣는다.
                        code = code.Replace(lastProp, lastProp + "\nelements.property(" + name + ", \"" + prop + "\", " + value + ")");
                    } else if (value != null) {
                        // properties 교체
                        string newProperties = propertiesMatch.Replace(getElementProp(name, prop), value);
                        code = code.Replace(propertiesMatch, newProperties);
                    } else if (propertiesMatch != "") {
                        code = code.Replace(propertiesMatch, "");
                    }

                    if (prop == "Name") {
                        code = code.Replace("DEFAULT_PT_" + value.Replace("\"", ""), getNameSpace(value.Replace("\"", "")) + "_PT_" + value.Replace("\"", ""));
                        code = code.Replace(name, value.Replace("\"", ""));
                    }
                }
            }
        }
        public string getCodeType(string name) {
            //물질들을 모두 체크
            foreach (Match elementMatch in Regex.Matches(code, "local ([A-Za-z0-9]{1,4})\\s*=\\s*elements.allocate\\(\"[A-Za-z_]+\",\\s*\"([A-Za-z0-9]{1,4})\"\\)")) {
                // 물질 이름을 가져온다.
                string elementName = elementMatch.ToString().Split(' ')[1];
                // 물질 이름과 name이 같으면 return "Element"
                if (elementName == name)
                    return "Element";
            }

            //뭣도 아니면 null
            return null;
        }
        //물질 생성하기
        public void newElement(){
            //물질 이름
            string name = newElementName();
            if (name == "")
                return;
            //새 물질 선언
            MatchCollection matches = Regex.Matches(code, "local ([A-Za-z0-9]{1,4})\\s*=\\s*elements.allocate\\(\"[A-Za-z_]+\",\\s*\"([A-Za-z0-9]{1,4})\"\\)");
            if (matches.Count > 0){
                string lastElementDeclaration = matches[matches.Count - 1].ToString();
                code = code.Replace(lastElementDeclaration, lastElementDeclaration + "\nlocal " + name + " = elements.allocate(\"OMEGATHREE\", \"" + name + "\")");
            } else {
                code = "local " + name + " = elements.allocate(\"OMEGATHREE\", \"" + name + "\")\n" + code;
            }
            //새 물질 코드 작성
            string newElementCode = "\n\nelements.element(" + name + ", elements.element(elements.DEFAULT_PT_BRMT))\n" +
                "elements.property(" + name + ", \"Name\", \"" + name + "\")\n" +
                "elements.property(" + name + ", \"Colour\", 0xFFFFFF)\n" +
                "elements.property(" + name + ", \"Description\", \"new elements\")\n" +
                "elements.property(" + name + ", \"State\", elem.PT_SOLID)\n" +
                "elements.property(" + name + ", \"MenuSection\", elem.SC_POWDERS)\n" +
                "elements.property(" + name + ", \"Properties\", elem.TYPE_PART)\n" +
                "elements.property(" + name + ", \"Weight\", 90)";
            //새 물질 코드 삽입
            matches = Regex.Matches(code, "elements\\.property\\(([A-Za-z0-9]{1,4}),\\s*\"[A-Za-z0-9]+\",\\s*[ -~]+\\)");
            if (matches.Count > 0){
                string lastProp = matches[matches.Count - 1].ToString();
                code = code.Replace(lastProp, lastProp + newElementCode);
            } else {
                code += newElementCode;
            }
        }
        //물질 제거하기
        public void deleteElement(string name) {
            if (getCodeType(name) != "Element")
                return;

            foreach (Match m in Regex.Matches(code, "local " + name + "\\s*=\\s*elements\\.allocate\\(\"[A-Za-z_]+\"\\s*,\\s*\"" + name + "\"\\)\n*")){
                code = code.Replace(m.ToString(), "");
            }
            foreach (Match m in Regex.Matches(code, "elements\\.element\\(" + name + ",\\s*elements\\.element\\(elements\\.DEFAULT_PT_[A-Za-z0-9]{1,4}\\)\\)\\s*\n*")){
                code = code.Replace(m.ToString(), "");
            }
            foreach (Match m in Regex.Matches(code, "elements\\.property\\(" + name + ",\\s*\"[A-Za-z]+\",\\s*[ -~]+\\)\n*")){
                code = code.Replace(m.ToString(), "");
            }
        }
        //새 코드 조각 이름
        public string newElementName(){
            string name = "NEW1";
            for (int n = 2; getCodeNameList().Contains(name) && n <= 1000; n++) {
                if (n / 100 > 0)
                    name = "N" + n;
                else if (n / 10 > 0)
                    name = "NE" + n;
                else
                    name = "NEW" + n;
            }
            if (name == "N1000")
                return "";
            return name;
        }
        public string newFunctionName(string type){
            string name = "new" + type + 1;
            for (int n = 2; getCodeNameList().Contains(name); n++) {
                name = "new" + type + n;
            }
            return name;
        }
        //코드조각 네임스페이스 이름
        public string getNameSpace(string name) {
            //물질들을 모두 체크
            string allocateMatch = Regex.Match(code, "local " + name + "\\s*=\\s*elements.allocate\\(\"[A-Za-z_]+\",\\s*\"([A-Za-z0-9]{1,4})\"\\)").ToString();
            string namespaceMatch = Regex.Match(allocateMatch, "\\(\"[A-Za-z_]+\",").ToString();
            if (namespaceMatch != "") {
                namespaceMatch = namespaceMatch.Substring(2, namespaceMatch.Length - 4);
                return namespaceMatch;
            } else {
                return "DEFAULT";
            }
        }
    }
}
