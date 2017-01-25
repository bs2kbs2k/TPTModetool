using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;

namespace modEdit
{
    public partial class ModEdit : Form
    {
        TitleForm titleForm;
        CodeManager codeManager;

        string readingFileName = "";
        bool edited = false;

        /******/
        /*시작*/
        /******/
        public ModEdit(TitleForm __titleForm)
        {
            InitializeComponent();
            titleForm = __titleForm;

            codeManager = new CodeManager();
        }
        //코드 리스트 정리
        private void refreshCodeList(List<string> nameList) {
            codeListBox.Items.Clear();
            foreach (string s in nameList) {
                codeListBox.Items.Add(s);
            }
        }

        /*********/
        /*탭 관리*/
        /*********/
        void loadCode(){
            //아무것도 선택되지 않았다면 return
            if (codeListBox.SelectedIndex == -1) { return; }
            //코드 조각의 이름
            string name = codeListBox.SelectedItem.ToString();
            //코드 조각의 타입을 불러옴
            string codeType = codeManager.getCodeType(name);
            if (codeType == "Element") {
                //타입이 Element라면 물질을 불러온다.
                elementName.Text = codeManager.getElementProp(name, "Name").Replace("\"", "");
                elementDescription.Text = codeManager.getElementProp(name, "Description").Replace("\"", "");

                //elementColor
                elementColor.Text = codeManager.getElementProp(name, "Colour").Replace("0x", "#");
                try { elementColor.BackColor = ColorTranslator.FromHtml(elementColor.Text); } catch (Exception) { elementColor.BackColor = Color.White; }
                if (elementColor.BackColor.GetBrightness() > 0.5f)
                    elementColor.ForeColor = Color.Black;
                else
                    elementColor.ForeColor = Color.White;

                elementType.Text = codeManager.getElementProp(name, "Type");
                elementTab.Text = codeManager.getElementProp(name, "MenuSection").Replace("elem.SC_", "");

                // Weight
                double weight = double.Parse(codeManager.getElementProp(name, "Weight"));
                weight = rangeItself(weight, -1, 101);
                elementWeight.Value = Convert.ToDecimal(weight);

                // Temp
                double temp = calc(codeManager.getElementProp(name, "Temperature")) - 273.15;
                temp = rangeItself(temp, -273.15, 9726.85);
                elementTemp.Value = Convert.ToDecimal(temp);

                // HotAir
                double hotAir = double.Parse(codeManager.getElementProp(name, "HotAir"));
                hotAir = rangeItself(hotAir, 0, 10);
                elementHotAir.Value = Convert.ToDecimal(hotAir);

                // Flammable
                double flammable = double.Parse(codeManager.getElementProp(name, "Flammable"));
                flammable = rangeItself(flammable, 0, 1000);
                elementFlammable.Value = Convert.ToDecimal(flammable);

                // Hardness
                double hardness = double.Parse(codeManager.getElementProp(name, "Hardness"));
                hardness = rangeItself(hardness, 0, 100);
                elementHardness.Value = Convert.ToDecimal(hardness);

                // HeatConduct
                double heatConduct = double.Parse(codeManager.getElementProp(name, "HeatConduct"));
                heatConduct = rangeItself(heatConduct, 0, 255);
                elementConducts.Value = Convert.ToDecimal(heatConduct);

                // Properties
                elementHotGlow.Checked = codeManager.getElementProp(name, "Properties").Contains("PROP_HOT_GLOW");
                elementSprkConducts.Checked = codeManager.getElementProp(name, "Properties").Contains("PROP_CONDUCTS");
                elementNeutPass.Checked = codeManager.getElementProp(name, "Properties").Contains("PROP_NEUTPASS");
                elementNeutAbsorb.Checked = codeManager.getElementProp(name, "Properties").Contains("PROP_NEUTABSORB");
                elementDrawOnCtype.Checked = codeManager.getElementProp(name, "Properties").Contains("PROP_DRAWONCTYPE");

                // HighTemperature
                HTcheck.Checked = codeManager.getElementProp(name, "HighTemperature") != "N/A";
                if (HTcheck.Checked) {
                    double highTemperature = calc(codeManager.getElementProp(name, "HighTemperature")) - 273.15;
                    highTemperature = rangeItself(highTemperature, -273.15, 9726.85);
                    HT.Value = Convert.ToDecimal(highTemperature);
                } else {
                    HT.Value = 0;
                }
                HTT.Text = Regex.Replace(codeManager.getElementProp(name, "HighTemperatureTransition"), "elements\\.[A-Za-z_]+_PT_", "");

                // LowTemperature
                LTcheck.Checked = codeManager.getElementProp(name, "LowTemperature") != "N/A";
                if (LTcheck.Checked) {
                    double lowTemperature = calc(codeManager.getElementProp(name, "LowTemperature")) - 273.15;
                    lowTemperature = rangeItself(lowTemperature, -273.15, 9726.85);
                    LT.Value = Convert.ToDecimal(lowTemperature);
                } else {
                    LT.Value = 0;
                }
                LTT.Text = Regex.Replace(codeManager.getElementProp(name, "LowTemperatureTransition"), "elements\\.[A-Za-z_]+_PT_", "");

                // HighPressure
                HPcheck.Checked = codeManager.getElementProp(name, "HighPressure") != "N/A";
                if (HPcheck.Checked) {
                    double highPressure = double.Parse(codeManager.getElementProp(name, "HighPressure"));
                    highPressure = rangeItself(highPressure, -256, 256);
                    HP.Value = Convert.ToDecimal(highPressure);
                } else {
                    HP.Value = 0;
                }
                HPT.Text = Regex.Replace(codeManager.getElementProp(name, "HighPressureTransition"), "elements\\.[A-Za-z_]+_PT_", "");

                // LowPressure
                LPcheck.Checked = codeManager.getElementProp(name, "LowPressure") != "N/A";
                if (LPcheck.Checked) {
                    double lowPressure = double.Parse(codeManager.getElementProp(name, "LowPressure"));
                    lowPressure = rangeItself(lowPressure, -256, 256);
                    LP.Value = Convert.ToDecimal(lowPressure);
                } else {
                    LP.Value = 0;
                }
                LPT.Text = Regex.Replace(codeManager.getElementProp(name, "LowPressureTransition"), "elements\\.[A-Za-z_]+_PT_", "");
                syntax();
            }
        }
        bool saveCode() {
            //아무것도 선택되지 않았다면 return
            if (codeListBox.SelectedIndex == -1) { return false; }
            //코드 조각의 이름
            string codeName = codeListBox.SelectedItem.ToString();
            //코드 조각의 타입을 불러옴
            string codeType = codeManager.getCodeType(codeName);
            if (codeType == "Element") {
                if (!ElementSyntax.isElement(elementName.Text)) { MessageBox.Show("물질 이름은 로마자, 숫자로 1~4글자로 입력해 주세요.", "TPT 모드 편집기"); return false; }
                if (codeListBox.SelectedItem.ToString() != elementName.Text && codeListBox.Items.Contains(elementName.Text)) { MessageBox.Show("물질 이름이 중복됩니다.", "TPT 모드 편집기"); return false; }

                if (!ElementSyntax.isElement(HTT.Text) && HTcheck.Checked) { MessageBox.Show("물질 이름은 로마자, 숫자로 1~4글자로 입력해 주세요.", "TPT 모드 편집기"); return false; }
                if (!ElementSyntax.isElement(LTT.Text) && LTcheck.Checked) { MessageBox.Show("물질 이름은 로마자, 숫자로 1~4글자로 입력해 주세요.", "TPT 모드 편집기"); return false; }
                if (!ElementSyntax.isElement(HPT.Text) && HPcheck.Checked) { MessageBox.Show("물질 이름은 로마자, 숫자로 1~4글자로 입력해 주세요.", "TPT 모드 편집기"); return false; }
                if (!ElementSyntax.isElement(LPT.Text) && LPcheck.Checked) { MessageBox.Show("물질 이름은 로마자, 숫자로 1~4글자로 입력해 주세요.", "TPT 모드 편집기"); return false; }
                if (!ElementSyntax.isDescription(elementDescription.Text)) { MessageBox.Show("물질 설명은 로마자와 문장 부호('.', ',', '?', '!', '-', ':', ';', ''')로 입력해 주세요.", "TPT 모드 편집기"); return false; }
                if (!ElementSyntax.isType(elementType.Text)) { MessageBox.Show("\'종류\'값을 올바르게 입력해 주세요.", "TPT 모드 편집기"); return false; }
                if (!ElementSyntax.isTab(elementTab.Text)) { MessageBox.Show("\'탭\'값을 올바르게 입력해 주세요.", "TPT 모드 편집기"); return false; }

                //타입이 Element라면 물질을 적용한다.
                string name = elementName.Text;
                codeManager.setElementProp(codeName, "Name", "\"" + elementName.Text + "\"");
                codeManager.setElementProp(name, "Description", "\"" + elementDescription.Text + "\"");
                codeManager.setElementProp(name, "Colour", "0x" + elementColor.Text.Replace("#", ""));
                codeManager.setElementProp(name, "Type", elementType.Text);
                codeManager.setElementProp(name, "MenuSection", "elem.SC_" + elementTab.Text);
                codeManager.setElementProp(name, "Weight", elementWeight.Value.ToString());
                codeManager.setElementProp(name, "Temperature", (elementTemp.Value+273.15M).ToString());
                codeManager.setElementProp(name, "HotAir", elementHotAir.Value.ToString());
                codeManager.setElementProp(name, "Flammable", elementFlammable.Value.ToString());
                codeManager.setElementProp(name, "Hardness", elementHardness.Value.ToString());
                codeManager.setElementProp(name, "HeatConduct", elementConducts.Value.ToString());

                // Properties
                string properties = "elem.TYPE_" + ((elementType.Text == "POWDER") ? "PART" : elementType.Text);
                properties += elementHotGlow.Checked ? " + elem.PROP_HOT_GLOW" : "";
                properties += elementSprkConducts.Checked ? " + elem.PROP_CONDUCTS + elem.PROP_LIFE_DEC" : "";
                properties += elementNeutPass.Checked ? " + PROP_NEUTPASS" : "";
                properties += elementNeutAbsorb.Checked ? " + PROP_NEUTABSORB" : "";
                properties += elementDrawOnCtype.Checked ? " + PROP_DRAWONCTYPE" : "";
                codeManager.setElementProp(name, "Properties", properties);

                // HighTemperature
                if (HTcheck.Checked) {
                    codeManager.setElementProp(name, "HighTemperature", (HT.Value + 273.15M).ToString());
                    codeManager.setElementProp(name, "HighTemperatureTransition", "elements." + codeManager.getNameSpace(HTT.Text) + "_PT_" + HTT.Text);
                } else {
                    codeManager.setElementProp(name, "HighTemperature", null);
                    codeManager.setElementProp(name, "HighTemperatureTransition", null);
                }

                // LowTemperature
                if (LTcheck.Checked) {
                    codeManager.setElementProp(name, "LowTemperature", (LT.Value + 273.15M).ToString());
                    codeManager.setElementProp(name, "LowTemperatureTransition", "elements." + codeManager.getNameSpace(LTT.Text) + "_PT_" + LTT.Text);
                } else {
                    codeManager.setElementProp(name, "LowTemperature", null);
                    codeManager.setElementProp(name, "LowTemperatureTransition", null);
                }

                // HighPressure
                if (HPcheck.Checked) {
                    codeManager.setElementProp(name, "HighPressure", HP.Value.ToString());
                    codeManager.setElementProp(name, "HighPressureTransition", "elements." + codeManager.getNameSpace(HPT.Text) + "_PT_" + HPT.Text);
                } else {
                    codeManager.setElementProp(name, "HighPressure", null);
                    codeManager.setElementProp(name, "HighPressureTransition", null);
                }

                // LowPressure
                if (LPcheck.Checked) {
                    codeManager.setElementProp(name, "LowPressure", LP.Value.ToString());
                    codeManager.setElementProp(name, "LowPressureTransition", "elements." + codeManager.getNameSpace(LPT.Text) + "_PT_" + LPT.Text);
                } else {
                    codeManager.setElementProp(name, "LowPressure", null);
                    codeManager.setElementProp(name, "LowPressureTransition", null);
                }
                loadCode();
            }
            return true;
        }
        //선택된 코드 조각이 바뀔 때마다 불러옴
        private void codeListBox_SelectedIndexChanged(object sender, EventArgs e){
            elementBox.Visible = true;
            loadCode();
        }
        //적용 버튼 클릭될 때마다 저장
        private void elementApply_Click(object sender, EventArgs e){
            edited = true;
            bool saved = saveCode();
            if (saved) {
                refreshCodeList(codeManager.getCodeNameList());
                codeListBox.SelectedItem = elementName.Text;
            }
        }

        /******************/
        /*물질 리스트 관리*/
        /******************/
        //물질 생성
        private void toolStripButton5_Click(object sender, EventArgs e){
            edited = true;
            string name = codeManager.newElementName();
            codeManager.newElement();
            //새로고침
            refreshCodeList(codeManager.getCodeNameList());
            codeListBox.SelectedItem = name;
            elementBox.Visible = true;
        }
        //코드 제거
        private void toolStripButton8_Click(object sender, EventArgs e){
            //아무것도 선택되지 않았다면 return
            if (codeListBox.SelectedIndex == -1) { return; }
            //코드 조각의 이름
            string name = codeListBox.SelectedItem.ToString();
            //코드 조각의 타입을 불러옴
            string codeType = codeManager.getCodeType(name);
            //물질이면 물질 제거
            if (codeType == "Element"){
                codeManager.deleteElement(name);
            }
            //새로고침
            refreshCodeList(codeManager.getCodeNameList());
            elementBox.Visible = false;
        }
        /*************/
        /*파일 입출력*/
        /*************/
        // 새 파일
        private void newFile() {
            checkEdited();
            codeManager.setCode("");
            refreshCodeList(codeManager.getCodeNameList());
            readingFileName = "";
            setTitle();
            //박스 숨김
            elementBox.Visible = false;
        }
        // 불러오기
        private void load() {
            checkEdited();
            openFileDialog.ShowDialog();
            if (openFileDialog.CheckFileExists && openFileDialog.FileName != "") {
                readingFileName = openFileDialog.FileName;
                string code = System.IO.File.ReadAllText(openFileDialog.FileName);
                codeManager.setCode(code);
            }
            refreshCodeList(codeManager.getCodeNameList());
            setTitle();
            //박스 숨김
            elementBox.Visible = false;
        }
        // 저장
        private void save() {
            if (readingFileName != ""){
                System.IO.File.WriteAllText(readingFileName, codeManager.getCode());
                edited = false;
            } else {
                saveAt();
            }
            setTitle();
            //박스 숨김
            elementBox.Visible = false;
        }
        // 다른 이름으로 저장
        private void saveAt() {
            saveFileDialog.ShowDialog();
            if(saveFileDialog.FileName != "")
                System.IO.File.WriteAllText(saveFileDialog.FileName, codeManager.getCode());
            edited = false;
            //박스 숨김
            elementBox.Visible = false;
        }
        // 나가기 전 체크
        private void checkEdited(){
            if (edited){
                string filename = readingFileName != "" ? readingFileName.Split('\\')[readingFileName.Split('\\').Length - 1] : "제목 없음";
                DialogResult result = MessageBox.Show(filename + "에 편집된 내용을 저장하시겠습니까?", "TPT 모드편집툴", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                    save();
            }
        }
        //툴바 버튼
        private void toolStripButton1_Click(object sender, EventArgs e) { newFile(); }
        private void toolStripButton2_Click(object sender, EventArgs e) { load(); }
        private void toolStripButton3_Click(object sender, EventArgs e) { save(); }
        private void toolStripButton4_Click(object sender, EventArgs e) { saveAt(); }
        /******/
        /*종료*/
        /******/
        //닫힐 때 로딩폼도 같이 닫는다.
        private void ModEdit_FormClosing(object sender, FormClosingEventArgs e) { checkEdited(); titleForm.Close(); }
        /******/
        /*기타*/
        /******/
        //뭐 기타등등 함수들...
        double rangeItself(double n, double min, double max) {
            if (n < min)
                return min;
            else if (n > max)
                return max;
            else
                return n;
        }
        double calc(string expr) {
            return (double)new System.Xml.XPath.XPathDocument
            (new StringReader("<r/>")).CreateNavigator().Evaluate
            (string.Format("number({0})", new
            System.Text.RegularExpressions.Regex(@"([\+\-\*])")
            .Replace(expr, " ${1} ")
            .Replace("/", " div ")
            .Replace("%", " mod ")));
        }
        void setTitle() {
            this.Text = (readingFileName != "" ? readingFileName.Split('\\')[readingFileName.Split('\\').Length - 1] : "제목 없음") + " - TPT 모드 편집기";
        }
        /**********/
        /*체크박스*/
        /**********/
        private void HTcheck_CheckedChanged(object sender, EventArgs e) {
            HT.Enabled = HTT.Enabled = HTcheck.Checked;
            HT.ReadOnly = HTT.ReadOnly = !HTcheck.Checked;
            syntax();
        }
        private void LTcheck_CheckedChanged(object sender, EventArgs e) {
            LT.Enabled = LTT.Enabled = LTcheck.Checked;
            LT.ReadOnly = LTT.ReadOnly = !LTcheck.Checked;
            syntax();
        }
        private void HPcheck_CheckedChanged(object sender, EventArgs e) {
            HP.Enabled = HPT.Enabled = HPcheck.Checked;
            HP.ReadOnly = HPT.ReadOnly = !HPcheck.Checked;
            syntax();
        }
        private void LPcheck_CheckedChanged(object sender, EventArgs e) {
            LP.Enabled = LPT.Enabled = LPcheck.Checked;
            LP.ReadOnly = LPT.ReadOnly = !LPcheck.Checked;
            syntax();
        }
        /*************************************************/
        /*type이 바뀔때마다 나머지 프롭들을 알아서 정해줌*/
        /*************************************************/
        private void elementType_SelectedIndexChanged(object sender, EventArgs e){
            string type = elementType.Text;
            if (type == "GAS"){
                elementTab.Text = "GAS";
                elementWeight.Value = 1;
                elementHotAir.Value = 0.001m;
            } else if (type == "LIQUID") {
                elementTab.Text = "LIQUID";
                elementWeight.Value = 30;
                elementHotAir.Value = 0;
            } else if (type == "SOLID") {
                elementTab.Text = "SOLIDS";
                elementWeight.Value = 100;
                elementHotAir.Value = 0;
            } else if (type == "POWDER") {
                elementTab.Text = "POWDERS";
                elementWeight.Value = 90;
                elementHotAir.Value = 0;
            }
        }

        /***********/
        /*색 고르기*/
        /***********/
        private void elementColor_Click(object sender, EventArgs e){
            colorDialog.ShowDialog();
            Color selectedColor = colorDialog.Color;

            elementColor.BackColor = selectedColor;
            elementColor.Text = String.Format("#{0:X2}{1:X2}{2:X2}", selectedColor.R, selectedColor.G, selectedColor.B);
            if (selectedColor.GetBrightness() > 0.5f)
                elementColor.ForeColor = Color.Black;
            else
                elementColor.ForeColor = Color.White;
        }
        /***********/
        /*구문 분석*/
        /***********/
        private void syntax() {
            elementName.BackColor = 
                !ElementSyntax.isElement(elementName.Text) || 
                (codeListBox.SelectedItem.ToString() != elementName.Text && codeListBox.Items.Contains(elementName.Text)) ? Color.Red : Color.White;

            elementDescription.BackColor = !ElementSyntax.isDescription(elementDescription.Text) ? Color.Red : Color.White;
            HTT.BackColor = !ElementSyntax.isElement(HTT.Text) && HTcheck.Checked ? Color.Red : Color.White;
            LTT.BackColor = !ElementSyntax.isElement(LTT.Text) && LTcheck.Checked ? Color.Red : Color.White;
            HPT.BackColor = !ElementSyntax.isElement(HPT.Text) && HPcheck.Checked ? Color.Red : Color.White;
            LPT.BackColor = !ElementSyntax.isElement(LPT.Text) && LPcheck.Checked ? Color.Red : Color.White;
        }
        private void elementName_TextChanged(object sender, EventArgs e) { syntax(); }
        private void elementDescription_TextChanged(object sender, EventArgs e) { syntax(); }
        private void HTT_TextChanged(object sender, EventArgs e) { syntax(); }
        private void LTT_TextChanged(object sender, EventArgs e) { syntax(); }
        private void HPT_TextChanged(object sender, EventArgs e) { syntax(); }
        private void LPT_TextChanged(object sender, EventArgs e) { syntax(); }
    }
}

