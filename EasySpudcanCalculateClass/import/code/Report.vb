Imports Easy.EsWord
Imports System.Math, System.Drawing, System.Windows.Forms
Imports Easy.EasyPlot
Imports EasyFiniteElement.EasyStructure
Imports System.Security.Cryptography
Public Class Report
    Dim SETTING_DOCUMENT As EsWordDocument
    Dim HeaderColor, FooterColor As Color
    Dim HeaderFont, FooterFont, Heading1Font, Heading2Font, TableFont, BodyFont, CharacterFont As Font
    Dim HeaderAlign, FooterAlign As EsWordTextAlign
    Dim mydataset As DataSet
    Dim SectionNumber As Integer, SubSectionNumber As Integer, ChapterNumber As Integer
    Private Table(,) As String
    Public TableWidths() As Long
    Dim SoilNames As Dictionary(Of Integer, String)
    Private MainReportPath As String = ""
    Sub New(StructureKit As EasyStructureKit)
        SETTING_DOCUMENT = New EsWordDocument(EsWordDocumentFormat.A4)
        HeaderColor = Color.FromArgb(0, 0, 0)
        FooterColor = Color.FromArgb(0, 0, 0)
        HeaderFont = New Font("宋体", 9, FontStyle.Regular) '小五
        FooterFont = New Font("宋体", 9, FontStyle.Regular)
        Heading1Font = New Font("黑体", 14, FontStyle.Regular)
        Heading2Font = New Font("黑体", 12, FontStyle.Regular)
        TableFont = New Font("宋体", 10, FontStyle.Regular)
        BodyFont = New Font("宋体", 12, (FontStyle.Regular))
        CharacterFont = New Font("Times New Roman", 12, FontStyle.Regular)
        HeaderAlign = EsWordTextAlign.Center
        FooterAlign = EsWordTextAlign.Right
        mydataset = StructureKit.StructureData.GetData
        ChapterNumber = 0
        SoilNames = New Dictionary(Of Integer, String)


    End Sub
    ''' <summary>
    ''' 单船版报告
    ''' </summary>
    ''' <param name="OutputPath"></param>
    Public Sub BeginWrite(Optional ByVal OutputPath As String = "")
        On Error Resume Next
        Setting_HeaderAndFooter()
        Setting_Cover()

        Setting_Text_StructureData()
        Setting_Text_CalculationParameter()
        Setting_Text_Result()

        If OutputPath = "" Then
            Dim SaveReportDialog As New SaveFileDialog
            SaveReportDialog.Filter = "(*.doc)|*.doc"
            SaveReportDialog.ShowDialog()
            If SaveReportDialog.FileName <> "" Then
                If Dir(SaveReportDialog.FileName) <> "" Then
                    Kill(SaveReportDialog.FileName)
                End If
                SETTING_DOCUMENT.SaveToFile(SaveReportDialog.FileName)
            Else
                Exit Sub
            End If
            System.Diagnostics.Process.Start(SaveReportDialog.FileName)
        Else
            OutputPath &= "\计算报告.doc"
            If Dir(OutputPath) <> "" Then
                Kill(OutputPath)
            End If
            SETTING_DOCUMENT.SaveToFile(OutputPath)
        End If
    End Sub
    ''' <summary>
    ''' 多船版报告
    ''' </summary>
    ''' <param name="TaskName"></param>
    ''' <param name="ProjectName"></param>
    ''' <param name="OutputPath"></param>
    ''' <param name="UseMetaFile"></param>
    ''' <param name="DrillingID">钻孔编号，DrillingID=0时输出主报告，即所有钻孔结果，反之则输出分报告，即单个钻孔结果。</param>
    Public Sub BeginWrite_Template(ByVal TaskName As String, ByVal ProjectName As String, Optional ByVal OutputPath As String = "", Optional UseMetaFile As Boolean = True, Optional DrillingID As Integer = 0)
        On Error Resume Next
        Heading1Font = New Font("宋体", 16, FontStyle.Bold) 'New Font("Times New Roman", 14, FontStyle.Bold)
        Heading2Font = New Font("宋体", 14, FontStyle.Bold)
        BodyFont = New Font("宋体", 12, FontStyle.Regular)
        TableFont = New Font("宋体", 10.5, FontStyle.Regular)
        HeaderAlign = EsWordTextAlign.Right
        FooterAlign = EsWordTextAlign.Right

        If ProjectName = "" Then ProjectName = mydataset.Tables("LS_StructureData").Rows(0)("WindFieldName")
        Setting_Template_HeaderAndFooter(If(TaskName = "", ProjectName, TaskName))
        Setting_Template(ProjectName, DrillingID, UseMetaFile)
        Dim DrillingName As String = ""
        If OutputPath = "" Then
            '主报告
            If DrillingID = 0 Then
                Dim SaveReportDialog As New SaveFileDialog
                SaveReportDialog.Filter = "(*.doc)|*.doc"
                SaveReportDialog.ShowDialog()
                If SaveReportDialog.FileName <> "" Then
                    MainReportPath = SaveReportDialog.FileName
                    If Dir(SaveReportDialog.FileName) <> "" Then
                        Kill(SaveReportDialog.FileName)
                    End If
                    SETTING_DOCUMENT.SaveToFile(SaveReportDialog.FileName)
                Else
                    Exit Sub
                End If
                System.Diagnostics.Process.Start(SaveReportDialog.FileName)
            End If
        Else
            '分报告
            If DrillingID Then DrillingName = "_" & mydataset.Tables("LS_TempSoilDrilling").Select("DrillingID=" & DrillingID, "DrillingID")(0)("DrillingName")
            OutputPath &= "\" & If(TaskName = "", ProjectName, TaskName) & DrillingName & ".doc" '"\计算报告.doc"
            MainReportPath = OutputPath
            If Dir(OutputPath) <> "" Then
                Kill(OutputPath)
            End If
            SETTING_DOCUMENT.SaveToFile(OutputPath)
            'System.Diagnostics.Process.Start(OutputPath)
        End If
    End Sub
    Function GetMainReportPath() As String
        Return MainReportPath
    End Function
    Sub Setting_Template(ByVal ProjectName As String, DrillingID As Integer, Optional UseMetaFile As Boolean = True)
        Dim TabColNames As String()
        Dim TabColShowNames As String()
        Dim N As Integer
        Dim J As Integer
        'Dim LevelDic As New Dictionary(Of Integer, List(Of Double))
        'Dim DepthDic As New Dictionary(Of Integer, List(Of Double))
        'Dim Qu0DrillingNameDic As New Dictionary(Of Integer, List(Of String))
        'Dim Qu1DrillingNameDic As New Dictionary(Of Integer, List(Of String))
        'Dim Qu0OkDrillingNameDic As New Dictionary(Of Integer, List(Of String))
        ChapterNumber += 1
        SectionNumber = 0
        SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Center)
        SETTING_DOCUMENT.SetFont(Heading1Font)
        SETTING_DOCUMENT.WriteLine("计算报告")

        SectionNumber += 1
        SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Left)
        SETTING_DOCUMENT.SetFont(Heading2Font)
        SETTING_DOCUMENT.WriteLine(SectionNumber & ".计算参数")
        SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Justified)
        SETTING_DOCUMENT.SetFont(BodyFont)
        SETTING_DOCUMENT.WriteLine("项目名称：" & ProjectName)
        Dim DrillingName As String = ""
        Dim Rows As DataRow()
        If DrillingID <> 0 Then
            DrillingName = mydataset.Tables("LS_TempSoilDrilling").Select("DrillingID=" & DrillingID, "DrillingID")(0)("DrillingName")
            SETTING_DOCUMENT.WriteLine("计算钻孔：" & DrillingName)
        End If
        For Each Row In mydataset.Tables("LS_Boat").Select("IsCount=1")
            SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Center)
            SETTING_DOCUMENT.SetFont(TableFont)
            SETTING_DOCUMENT.WriteLine(Row("Name") & "船舶参数")
            Dim ColumnNames As String() = {"LegCircumference", "LegA", "SpudcanL", "SpudcanB", "SpudcanH", "SpudcanA", "SpudcanCircumference", "SpudcanV", "W", "LegPressForce", "SumW", "PullingCapacity", "GroundPressure", "LegHLN", "AirGap"} ', "LegActiveLength"20250929报告中移除该行
            Dim EColumnNames As String() = {"桩腿周长", "桩腿截面积（用于计算回流土体体积）", "桩靴长度 L", "桩靴宽度 B", "桩靴高度 H", "桩靴面积 A", "桩靴最大截面周长", "桩靴体积 V", "桩腿、桩靴自重 W", "桩腿预压力", "计算预压荷载", "拔桩力", "对地比压", "有效桩腿长度（船底到靴底）", "气隙（船底到水面）"} ', "桩腿有效长度"20250929报告中移除该行
            Dim Units As String() = {"m", "m{\super 2}", "m", "m", "m", "m{\super 2}", "m", "m{\super 3}", "t", "t", "t", "t", "kpa", "m", "m", "m"}
            N = ColumnNames.Length
            ReDim Table(N - 1, 2)
            ReDim TableWidths(2)
            For i = 0 To 2
                TableWidths(i) = 2000
            Next
            TableWidths(0) = 4000
            For i = 0 To N - 1
                Table(i, 0) = EColumnNames(i)
                Table(i, 1) = Round(Val(If(ColumnNames(i) = "SpudcanH", Row("SpudcanParameter").ToString.Split("=").Last, Row(ColumnNames(i)))), 2)
                Table(i, 2) = Units(i)
                If Row("SpudcanShapeType") = 0 Then
                    If EColumnNames(i) = "桩靴长度 L" Then
                        Table(i, 0) = "桩靴截面是否为圆形"
                        Table(i, 1) = "是"
                        Table(i, 2) = ""
                    End If
                    If EColumnNames(i) = "桩靴宽度 B" Then Table(i, 0) = "桩靴直径 B"
                End If
                If Row("LegType") = 2 Then
                    If EColumnNames(i) = "桩腿直径" Then
                        Table(i, 0) = "桁架式桩腿弦杆间距"
                    End If
                End If
            Next
            Call inset_a_table(SETTING_DOCUMENT, Table, 3, N, TableWidths)
            SETTING_DOCUMENT.WriteLine(Chr(13))
        Next
        If DrillingID <> 0 Then
            SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Center)
            SETTING_DOCUMENT.SetFont(TableFont)
            SETTING_DOCUMENT.WriteLine("土层参数")
            Rows = mydataset.Tables("LS_SoilDrillingParameter").Select("DrillingID=" & DrillingID & " and BoatID=1", "TopLevel DESC")
            ReDim Table(Rows.Count, 4)
            Table(0, 0) = "层顶高程"
            Table(0, 1) = "土层名称"
            Table(0, 2) = "浮重度（kN/m{\super 3}）"
            Table(0, 3) = "不排水抗剪强度Su（kPa）"
            Table(0, 4) = "摩擦角（°）"
            ReDim TableWidths(4)
            For i = 0 To 4
                TableWidths(i) = 900
            Next
            TableWidths(1) = 1500
            J = 0
            For Each Arow In Rows
                J += 1
                SoilNames.Add(Arow("ID"), Arow("Name"))
                Table(J, 0) = Arow("TopLevel")
                Table(J, 1) = Arow("Name")
                Table(J, 2) = Arow("UnderWaterWeight")
                Table(J, 3) = Arow("Su")
                Table(J, 4) = Arow("UnderWaterPhi")
            Next
            Call inset_a_table(SETTING_DOCUMENT, Table, 5, J + 1, TableWidths)
            SETTING_DOCUMENT.WriteLine(Chr(13)) '回车
        End If

        SectionNumber += 1
        SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Left)
        SETTING_DOCUMENT.SetFont(Heading2Font)
        SETTING_DOCUMENT.WriteLine(SectionNumber & ".计算说明")
        SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Justified)
        SETTING_DOCUMENT.SetFont(BodyFont)
        SETTING_DOCUMENT.WriteLine("（1）计算拔桩力时，底部持力层为粘土层时，考虑粘土强度恢复及固结和由于粘土渗透性差导致的吸附力；底部持力层为砂层时不考虑吸附力。
（2）冲桩减阻系统完全发挥作用时的拔桩力是假定桩靴周围土体均已发生破坏，即土体的抗剪强度为0。")

        SectionNumber += 1
        SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Left)
        SETTING_DOCUMENT.SetFont(Heading2Font)
        SETTING_DOCUMENT.WriteLine(SectionNumber & ".计算结果")
        SubSectionNumber = 0
        Dim DrillingDic As New Dictionary(Of Integer, List(Of Integer))
        '相同钻孔Dic将船ID做为key，由不同抗剪强度土的钻孔ID做为键，相同土层定义的不同钻孔ID做为值的Dic做为值
        Dim SameDrillingDic As New Dictionary(Of Integer, Dictionary(Of Integer, List(Of Integer))) '同一钻孔存在不同抗剪强度su的土层，按多个钻孔输入
        For Each Row In mydataset.Tables("LS_Boat").Select("IsCount=1", "ID")
            For Each Irow In mydataset.Tables("LS_TempSoilDrilling").Select("BoatID=" & Row("ID") & If(DrillingID, " and DrillingID=" & DrillingID, ""))
                If Not DrillingDic.ContainsKey(Row("ID")) Then
                    DrillingDic.Add(Row("ID"), New List(Of Integer))
                End If
                If Not DrillingDic(Row("ID")).Contains(Irow("DrillingID")) Then
                    DrillingDic(Row("ID")).Add(Irow("DrillingID"))
                End If
                Dim DriIDs As List(Of Integer) = SpudcanDB.GetDrillingIDs(mydataset, Irow("DrillingID"), Row("ID"))
                'If DriIDs.Count = 3 Then
                If Not SameDrillingDic.ContainsKey(Row("ID")) Then
                    SameDrillingDic.Add(Row("ID"), New Dictionary(Of Integer, List(Of Integer))) ' From {{Irow("DrillingID"), DriIDs}}
                    'If DriIDs(1) = Val(Irow("DrillingID")) Then
                    '    SameDrillingDic.Add(Row("ID"), New Dictionary(Of Integer, List(Of Integer)) From {{Irow("DrillingID"), DriIDs}})
                    'Else
                    '    SameDrillingDic.Add(Row("ID"), New Dictionary(Of Integer, List(Of Integer)) From {{Irow("DrillingID"), New List(Of Integer)}})
                    'End If 
                End If
                If Not SameDrillingDic(Row("ID")).ContainsKey(Irow("DrillingID")) And DriIDs.Count = 3 Then
                    SameDrillingDic(Row("ID")).Add(Irow("DrillingID"), DriIDs)
                End If
                'End If


                '合并不同抗剪强度的同一钻孔
                'If Not DrillingDic(Row("ID")).ContainsKey(Irow("DrillingID")) Then
                '    If (DriIDs.Count = 1 And DriIDs(0) = Val(Irow("DrillingID"))) Or DriIDs(1) = Val(Irow("DrillingID")) Then
                '        DrillingDic(Row("ID")).Add(Irow("DrillingID"), DriIDs)
                '    Else
                '        DrillingDic(Row("ID")).Add(Irow("DrillingID"), New List(Of Integer) From {DriIDs(1)})
                '    End If
                'End If
            Next
        Next
        For Each Row In mydataset.Tables("LS_Boat").Select("IsCount=1")
            'LevelDic.Add(Row("ID"), New List(Of Double))
            'DepthDic.Add(Row("ID"), New List(Of Double))
            'Qu0DrillingNameDic.Add(Row("ID"), New List(Of String))
            'Qu1DrillingNameDic.Add(Row("ID"), New List(Of String))
            'Qu0OkDrillingNameDic.Add(Row("ID"), New List(Of String))
            SubSectionNumber += 1
            SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Justified)
            SETTING_DOCUMENT.SetParagraph(2) '
            SETTING_DOCUMENT.SetFont(BodyFont)
            SETTING_DOCUMENT.WriteLine("(" & SubSectionNumber & ")" & Row("Name"))
            If DrillingID Then SETTING_DOCUMENT.WriteLine("插桩力计算结果：")
            If DrillingID = 0 Then
                Rows = mydataset.Tables("LS_DepthResult").Select("IsUserAdd=0 and BoatID=" & Row("ID"))
                TabColNames = {"DrillingID", "", "", "", "SuggestedDepth", "", "SupportSoilID", "Qv", "Qu0", "Qu1"}
                TabColShowNames = {"机位号", "勘察孔", "平台船名", "泥面标高（m）", "插桩标高（m）", "理论计算插深（m）", "持力层", "桩靴底部地基承载力（kPa）", "冲桩系统完全发挥作用时的拔桩力（t）", "不计减阻系统的最大拔桩力（t）"}

            Else
                Rows = mydataset.Tables("LS_PressResistanceResult").Select("BoatID=" & Row("ID") & " and DrillingID=" & DrillingID)
                TabColNames = {"DrillingID", "", "", "Level", "", "SoilID", "SelectMode", "Qv"}
                TabColShowNames = {"机位号", "勘察孔", "泥面标高（m）", "插桩标高（m）", "插深（m）", "持力层", "计算模式", "桩靴底部地基承载力（kPa）"}
            End If
            Dim TabTitles As New List(Of String)
            Dim TabTitleSuffixes As String() = {"强度小值", "强度中值", "强度大值"}
            Dim TabTitle As String = "表" & SectionNumber & "." & SubSectionNumber & " " & Row("Name") & "计算" & If(DrillingID = 0, "机位建议插桩深度与对应的拔桩力汇总", DrillingName & "机位不同插桩深度与对应的承载力")
            Dim GraphNumber As Integer = 0
            Dim ShowOneTab As Boolean = DrillingID > 0 OrElse SameDrillingDic(Row("ID")).Count = 0
            For SufI = 0 To If(ShowOneTab, 0, 2)
                If Not ShowOneTab Then
                    GraphNumber += 1
                    TabTitle = "表" & SectionNumber & "." & SubSectionNumber & "-" & GraphNumber & " " & Row("Name") & "计算" & If(DrillingID = 0, "机位建议插桩深度与对应的拔桩力汇总", DrillingName & "机位不同插桩深度与对应的承载力")
                End If
                TabTitles.Add(TabTitle & If(ShowOneTab, "", "（" & TabTitleSuffixes(SufI) & "）"))
            Next

            For Each ATabTitle In TabTitles
                SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Center)
                SETTING_DOCUMENT.SetFont(TableFont)
                SETTING_DOCUMENT.WriteLine(ATabTitle)
                N = TabColShowNames.Length
                ReDim Table(If(DrillingID, Rows.Count, DrillingDic(Row("ID")).Count), N - 1) 'Rows.Count
                J = 0
                For i = 0 To N - 1
                    Table(J, i) = TabColShowNames(i)
                Next
                ReDim TableWidths(N - 1)
                For i = 0 To N - 1
                    TableWidths(i) = 900
                Next
                TableWidths(0) = 400
                Dim NoteNoResult As String = ""
                '主报告和分报告的地基承载力
                For di = 0 To DrillingDic(Row("ID")).Count - 1
                    Dim NoResult As Boolean = True
                    For Each Irow In Rows
                        If Irow("DrillingID") = DrillingDic(Row("ID"))(di) Then
                            If DrillingID > 0 OrElse Not SameDrillingDic(Row("ID")).ContainsKey(Irow("DrillingID")) OrElse (SameDrillingDic(Row("ID")).ContainsKey(Irow("DrillingID")) AndAlso Irow("DrillingID") = SameDrillingDic(Row("ID"))(Irow("DrillingID"))(TabTitles.IndexOf(ATabTitle))) Then
                                '当相同钻孔Dic中不包含不同抗剪强度土层的钻孔ID或者当前钻孔ID为对应抗剪强度土层的钻孔ID，此时在表格中进行显示
                                GetResult(J, Row, Irow, N, TabColNames, TabColShowNames, True)
                                NoResult = False
                                If DrillingID = 0 Then
                                    Exit For
                                End If
                            End If
                        End If
                    Next
                    '
                    If NoResult Then
                        'J += 1
                        'Table(J, 0) = DrillingDic(Row("ID"))(di)
                        'Table(J, 1) = mydataset.Tables("LS_TempSoilDrilling").Select("BoatID=" & Row("ID") & " and DrillingID=" & DrillingDic(Row("ID"))(di))(0)("DrillingName")
                        'Table(J, 2) = Row("Name")
                        'Table(J, 3) = mydataset.Tables("LS_SoilDrillingParameter").Compute("Max(TopLevel)", "BoatID=" & Row("ID") & " and DrillingID=" & DrillingDic(Row("ID"))(di))
                        'For i = 4 To N - 1
                        '    Table(J, i) = "\"
                        'Next
                        'NoteNoResult &= Table(J, 1) & ","
                    End If
                Next
                Call inset_a_table(SETTING_DOCUMENT, Table, N, J + 1, TableWidths)
                SETTING_DOCUMENT.WriteLine(Chr(13))
                SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Justified)
                SETTING_DOCUMENT.SetParagraph(2) '
                SETTING_DOCUMENT.SetFont(BodyFont)
                If NoteNoResult <> "" Then SETTING_DOCUMENT.WriteLine("  注：对于机位" & NoteNoResult & "输入的地勘各土层不足，计算的地基承载力均小于桩靴对地压强，计算未得到上述机位的理论计算插深及持力层，同理无法输出上拔力计算结果。")

                Dim CurveTable As New EsPLCurveTable
                '分报告承载力图示
                If DrillingID Then
                    SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Center)
                    SETTING_DOCUMENT.SetFont(TableFont)
                    Dim LimitValue1 As Double = Row("GroundPressure")
                    CurveTable = New EsPLCurveTable
                    SETTING_DOCUMENT.PutImage(SpudcanDB.DrawPressCurve(mydataset, CurveTable, LimitValue1, 660, 659, 1, 3, DrillingID, Row("ID"), UseMetaFile), 562, 561)
                    SETTING_DOCUMENT.WriteLine(Chr(13))
                End If
                '分报告拔桩力
                If DrillingID Then
                    SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Justified)
                    SETTING_DOCUMENT.SetParagraph(2) '
                    SETTING_DOCUMENT.SetFont(BodyFont)
                    SETTING_DOCUMENT.WriteLine("拔桩力计算结果：")
                    SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Center)
                    SETTING_DOCUMENT.SetFont(TableFont)
                    SETTING_DOCUMENT.WriteLine("表" & SectionNumber & "." & SubSectionNumber + 1 & " " & Row("Name") & "计算" & DrillingName & "机位不同深度与对应的抗拔力")
                    TabColNames = {"DrillingID", "", "", "Level", "", "SoilID", "DeepType", "Qu0", "Qu1"}
                    TabColShowNames = {"机位号", "勘察孔", "泥面标高（m）", "插桩标高（m）", "插深（m）", "进入土层", "计算模式", "冲桩系统完全发挥作用时的拔桩力（t）", "不计减阻系统的最大拔桩力（t）"}

                    N = TabColShowNames.Length
                    Rows = mydataset.Tables("LS_PullResistanceResult").Select("BoatID=" & Row("ID") & " and DrillingID=" & DrillingID)
                    ReDim Table(Rows.Count, N - 1)
                    J = 0
                    For i = 0 To N - 1
                        Table(J, i) = TabColShowNames(i)
                    Next
                    ReDim TableWidths(N - 1)
                    For i = 0 To N - 1
                        TableWidths(i) = 900
                    Next
                    TableWidths(0) = 400
                    For Each Irow In Rows
                        GetResult(J, Row, Irow, N, TabColNames, TabColShowNames, False)
                    Next
                    Call inset_a_table(SETTING_DOCUMENT, Table, N, J + 1, TableWidths)
                    SETTING_DOCUMENT.WriteLine(Chr(13))
                    SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Justified)
                    SETTING_DOCUMENT.SetParagraph(2) '
                    SETTING_DOCUMENT.SetFont(BodyFont)
                    SETTING_DOCUMENT.WriteLine("桩靴底部地基承载力随插深标高变化情况见下图：")

                    SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Center)
                    SETTING_DOCUMENT.SetFont(TableFont)
                    CurveTable = New EsPLCurveTable
                    CurveTable.Curves.Clear()
                    SETTING_DOCUMENT.PutImage(SpudcanDB.DrawPullCurve(mydataset, CurveTable, 660, 659, 2, 1, DrillingID), 562, 561)
                    SETTING_DOCUMENT.WriteLine(Chr(13))
                    'SETTING_DOCUMENT.WriteLine(DrillingName & "拔桩力曲线")
                    'SETTING_DOCUMENT.WriteLine(Chr(13))
                End If
            Next
            '主报告承载力图示说明
            If DrillingID = 0 Then
                SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Justified)
                SETTING_DOCUMENT.SetFont(BodyFont)
                SETTING_DOCUMENT.WriteLine("桩靴底部地基承载力随插深标高变化情况见附录一。")
                SETTING_DOCUMENT.WriteLine(Chr(13))
            End If
            '穿刺风险评估
            If DrillingID = 0 Then
                SubSectionNumber += 1
                SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Center)
                SETTING_DOCUMENT.SetFont(TableFont)
                SETTING_DOCUMENT.WriteLine("表" & SectionNumber & "." & SubSectionNumber & " " & Row("Name").ToString & "穿刺相对安全系数结果")

                TabColNames = {"DrillingID", "P1", "P2", "P3", "Fs1", "Fs2", "IsPunctureRiskOK"}
                TabColShowNames = {"孔位", "P1（kPa）", "P2（kPa）", "P3（kPa）", "Fs1", "Fs2", "是否满足"}
                Rows = mydataset.Tables("LS_PunctureRiskAssessmentResult").Select("BoatID=" & Integer.Parse(Row("ID").ToString))

                N = TabColShowNames.Length
                ReDim Table(DrillingDic(Integer.Parse(Row("ID").ToString)).Count, N - 1) 'Rows.Count
                J = 0
                For i = 0 To N - 1
                    Table(J, i) = TabColShowNames(i)
                Next
                ReDim TableWidths(N - 1)
                For i = 0 To N - 1
                    TableWidths(i) = 900
                Next
                For di = 0 To DrillingDic(Integer.Parse(Row("ID").ToString)).Count - 1
                    Dim NoResult As Boolean = True
                    For Each Irow In Rows
                        If Integer.Parse(Irow("DrillingID").ToString) = DrillingDic(Integer.Parse(Row("ID").ToString))(di) Then
                            J += 1
                            Table(J, 0) = mydataset.Tables("LS_TempSoilDrilling").Select("BoatID=" & Row("ID").ToString & " and DrillingID=" & DrillingDic(Integer.Parse(Row("ID").ToString))(di).ToString)(0)("DrillingName").ToString
                            Table(J, 1) = Irow("P1").ToString
                            Table(J, 2) = Irow("P2").ToString
                            Table(J, 3) = Irow("P3").ToString
                            Table(J, 4) = Irow("Fs1").ToString
                            Table(J, 5) = Irow("Fs2").ToString
                            Table(J, 6) = If(Boolean.Parse(Irow("IsPunctureRiskOK").ToString()), "是", "否")
                            NoResult = False
                        End If
                    Next
                    '
                    If NoResult Then

                    End If
                Next
                Call inset_a_table(SETTING_DOCUMENT, Table, N, J + 1, TableWidths)
                SETTING_DOCUMENT.WriteLine(Chr(13))
                SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Justified)
                SETTING_DOCUMENT.SetParagraph(2) '
                SETTING_DOCUMENT.SetFont(BodyFont)
                SETTING_DOCUMENT.WriteLine("  注：穿刺相对安全系数按《海洋井场调查规范》有关规定计算。")
            End If
        Next
        Setting_Template_Conclusion(DrillingID)
        If DrillingID = 0 Then
            SETTING_DOCUMENT.NewPage()
            SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Left)
            SETTING_DOCUMENT.SetFont(Heading2Font)
            SETTING_DOCUMENT.WriteLine("附录一")
            SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Justified)
            SETTING_DOCUMENT.SetParagraph(2) '
            SETTING_DOCUMENT.SetFont(BodyFont)
            SETTING_DOCUMENT.WriteLine("桩靴底部地基承载力随插深标高变化情况见下图。")
            Dim CurveTable As New EsPLCurveTable
            For Each Row In mydataset.Tables("LS_Boat").Select("IsCount=1")
                For di = 0 To DrillingDic(Row("ID")).Count - 1
                    Dim ADrillingID As Integer = DrillingDic(Row("ID"))(di)
                    If Not SameDrillingDic(Row("ID")).ContainsKey(ADrillingID) OrElse ADrillingID = SameDrillingDic(Row("ID"))(ADrillingID)(1) Then
                        SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Center)
                        SETTING_DOCUMENT.SetFont(TableFont)
                        'Dim LimitValue1 As Double = mydataset.Tables("LS_DepthResult").Select("IsUserAdd=0 and BoatID=" & Row("ID") & " and DrillingID=" & DrillingID)(0)("LimitForce")
                        Dim LimitValue1 As Double = Row("GroundPressure")
                        CurveTable = New EsPLCurveTable
                        SETTING_DOCUMENT.PutImage(SpudcanDB.DrawPressCurve(mydataset, CurveTable, LimitValue1, 660, 659, 1, 3, ADrillingID, Row("ID"), UseMetaFile), 562, 561)
                        'SETTING_DOCUMENT.PutImage(SpudcanDB.DrawPressCurve(mydataset, CurveTable, LimitValue1, 660, 1000, 1, 3, ADrillingID, Row("ID"), UseMetaFile), 627, 950)
                        SETTING_DOCUMENT.WriteLine(Chr(13))
                    End If
                Next
            Next
        End If
    End Sub

    Sub Setting_Template_Conclusion(DrillingID As Integer)
        Dim LevelDic As New Dictionary(Of Integer, List(Of Double))
        Dim DepthDic As New Dictionary(Of Integer, List(Of Double))
        Dim PunctureRiskDic As New Dictionary(Of Integer, List(Of String))
        Dim Qu0DrillingNameDic As New Dictionary(Of Integer, List(Of String))
        Dim Qu1DrillingNameDic As New Dictionary(Of Integer, List(Of String))
        Dim Qu0OkDrillingNameDic As New Dictionary(Of Integer, List(Of String))

        Dim DrillingDic As New Dictionary(Of Integer, List(Of Integer))
        For Each Row In mydataset.Tables("LS_Boat").Select("IsCount=1", "ID")
            For Each Irow In mydataset.Tables("LS_TempSoilDrilling").Select("BoatID=" & Row("ID") & If(DrillingID, " and DrillingID=" & DrillingID, ""))
                If Not DrillingDic.ContainsKey(Row("ID")) Then
                    DrillingDic.Add(Row("ID"), New List(Of Integer))
                End If
                If Not DrillingDic(Row("ID")).Contains(Irow("DrillingID")) Then
                    DrillingDic(Row("ID")).Add(Irow("DrillingID"))
                End If
            Next
        Next
        For Each Row In mydataset.Tables("LS_Boat").Select("IsCount=1")
            LevelDic.Add(Row("ID"), New List(Of Double))
            DepthDic.Add(Row("ID"), New List(Of Double))
            PunctureRiskDic.Add(Row("ID"), New List(Of String))
            Qu0DrillingNameDic.Add(Row("ID"), New List(Of String))
            Qu1DrillingNameDic.Add(Row("ID"), New List(Of String))
            Qu0OkDrillingNameDic.Add(Row("ID"), New List(Of String))
            Dim Rows As DataRow() = mydataset.Tables("LS_DepthResult").Select("IsUserAdd=0 and BoatID=" & Row("ID"))
            For di = 0 To DrillingDic(Row("ID")).Count - 1
                For Each Irow In Rows
                    If Irow("DrillingID") = DrillingDic(Row("ID"))(di) Then
                        LevelDic(Row("ID")).Add(Irow("SuggestedDepth")) '"插桩标高（m）""SuggestedDepth"
                        Dim MudLevel As Double = mydataset.Tables("LS_SoilDrillingParameter").Compute("Max(TopLevel)", "BoatID=" & Row("ID") & " and DrillingID=" & Irow("DrillingID"))
                        Dim Level As Double = Irow("SuggestedDepth")
                        DepthDic(Row("ID")).Add(MudLevel - Level) '"理论计算插深（m）"
                        Dim DName As String = mydataset.Tables("LS_TempSoilDrilling").Select("BoatID=" & Row("ID") & " and DrillingID=" & Irow("DrillingID"))(0)("DrillingName")
                        If Double.Parse(Irow("Qu0")) / 9.8 >= Row("PullingCapacity") Then
                            Qu0DrillingNameDic(Row("ID")).Add(DName) '"勘察孔"
                        Else
                            Qu0OkDrillingNameDic(Row("ID")).Add(DName) '"勘察孔"
                        End If
                        If Double.Parse(Irow("Qu1")) / 9.8 >= Row("PullingCapacity") Then
                            Qu1DrillingNameDic(Row("ID")).Add(DName) '"勘察孔" 
                        End If
                        Exit For
                    End If
                Next

                Dim PRRows As DataRow() = mydataset.Tables("LS_PunctureRiskAssessmentResult").Select("BoatID=" & Row("ID"))
                For Each Irow In PRRows
                    If Irow("DrillingID") = DrillingDic(Row("ID"))(di) Then
                        If Not Irow("IsPunctureRiskOK") Then
                            Dim DName As String = mydataset.Tables("LS_TempSoilDrilling").Select("BoatID=" & Row("ID") & " and DrillingID=" & Irow("DrillingID"))(0)("DrillingName")
                            PunctureRiskDic(Row("ID")).Add(DName)
                        End If
                    End If
                Next
            Next
        Next
        SectionNumber += 1
        SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Left)
        SETTING_DOCUMENT.SetFont(Heading2Font)
        SETTING_DOCUMENT.WriteLine(SectionNumber & ".结论与建议")
        SubSectionNumber = 0
        SubSectionNumber += 1
        SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Justified)
        SETTING_DOCUMENT.SetParagraph(2) '
        SETTING_DOCUMENT.SetFont(BodyFont)
        SETTING_DOCUMENT.WriteLine("(" & SubSectionNumber & ")插深方面：")
        For Each Row In mydataset.Tables("LS_Boat").Select("IsCount=1")
            Dim Level As List(Of Double) = LevelDic(Row("ID"))
            Dim Depth As List(Of Double) = DepthDic(Row("ID"))
            SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Justified)
            SETTING_DOCUMENT.SetParagraph(2) '
            SETTING_DOCUMENT.SetFont(BodyFont)
            SETTING_DOCUMENT.WriteLine(Row("Name") & "：预压荷载为" & Round(Row("SumW"), 2) & "t时，桩靴对地压强为" & Round(Row("GroundPressure"), 2) & "kPa。" &
                                       If(Depth.Count = 0, "无理论插深结果", "理论计算插深为" & If(Depth.Count = 1, Round(Depth.Min, 2), Round(Depth.Min, 2) & "~" & Round(Depth.Max, 2))) & "m，" & If(Level.Count = 0, "无标高结果", "标高为" & If(Level.Count = 1, Round(Level.Min, 2), Round(Level.Max, 2) & "~" & Round(Level.Min, 2))) & "m。在同一机位的不同钻孔地层分布变化较大，相应的插深变化较大，施工中应需注意地层变化带来的施工风险。需结合插深和实际气隙等情况综合判断桩腿长度是否满足要求。")
        Next
        SETTING_DOCUMENT.WriteLine(Chr(13))
        SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Justified)
        SETTING_DOCUMENT.SetParagraph(2) '
        SETTING_DOCUMENT.SetFont(BodyFont)
        SETTING_DOCUMENT.WriteLine("此外，应注意以下两个因素：1）船机实际插腿位置的地层分布情况与勘察报告、钻孔报告显示的分布情况可能有所不同；2）桩靴下方持力层发生一定压缩量。")
        SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Justified)
        SETTING_DOCUMENT.SetParagraph(2) '
        SETTING_DOCUMENT.SetFont(New Font("宋体", 12, FontStyle.Underline))
        SETTING_DOCUMENT.WriteLine("以上因素可能导致实际插深与计算插深、预测插深有些差异，无法做到精确预测，尤其是在缺乏现场实操数据的情况下，故桩腿长度应留有一定富余量。建议开展典型工艺试验，并将工艺试验结果反馈至技术中心，以便结合原位测试资料对插深及拔桩力进行修正分析。")

        SETTING_DOCUMENT.WriteLine(Chr(13))
        SubSectionNumber += 1
        SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Justified)
        SETTING_DOCUMENT.SetParagraph(2) '
        SETTING_DOCUMENT.SetFont(BodyFont)
        SETTING_DOCUMENT.WriteLine("(" & SubSectionNumber & ")拔桩力方面：")
        For Each Row In mydataset.Tables("LS_Boat").Select("IsCount=1")
            Dim Qu1DrillingName As List(Of String) = Qu1DrillingNameDic(Row("ID"))
            Dim Qu0DrillingName As List(Of String) = Qu0DrillingNameDic(Row("ID"))
            Dim Qu0OkDrillingName As List(Of String) = Qu0OkDrillingNameDic(Row("ID"))

            Dim Qu0String As String = ""
            Dim Qu1String As String = ""
            Dim Qu0OkString As String = ""
            For Each QS In Qu0DrillingName
                Qu0String &= QS & ","
            Next
            For Each QS In Qu0OkDrillingName
                Qu0OkString &= QS & ","
            Next
            If Qu0DrillingName.Count > 0 Then
                Qu0String = Qu0String.Remove(Qu0String.Length - 1, 1)
                Qu0String &= "(占比" & Round(Qu0DrillingName.Count / (Qu0DrillingName.Count + Qu1DrillingName.Count) * 100, 2) & "%)"
            End If
            If Qu0OkDrillingName.Count > 0 Then
                Qu0OkString = Qu0OkString.Remove(Qu0OkString.Length - 1, 1)
                Qu0OkString &= "(占比" & Round(Qu0OkDrillingName.Count / (Qu0DrillingName.Count + Qu1DrillingName.Count) * 100, 2) & "%)"
            End If
            For Each QS In Qu1DrillingName
                Qu1String &= QS & ","
            Next
            If Qu1DrillingName.Count > 0 Then
                Qu1String = Qu1String.Remove(Qu1String.Length - 1, 1)
            End If
            SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Justified)
            SETTING_DOCUMENT.SetParagraph(2) '
            SETTING_DOCUMENT.SetFont(BodyFont)
            SETTING_DOCUMENT.WriteLine(Row("Name") & "：不计减阻系统的最大拔桩阻力" & If(Qu1String <> "", "超过最大拔桩能力，" & Qu1String, "小于最大拔桩能力，未") & "存在拔桩力不足的问题。由于船体自带冲桩减阻系统，冲桩减阻系统完全发挥作用时，假定桩靴周围土体均已发生破坏，即土体的抗剪强度为0。
若考虑冲桩减阻系统完全发挥作用，" & If(Qu0String = "", "所有机位", If(Qu0OkString = "", "所有机位均不", Qu0OkString)) & "满足拔桩力要求" & If(Qu0String = "", "。", If(Qu0OkString = "", "，", "，其余" & Qu0String) & "需特别注意拔桩能力问题。"))
        Next


        SETTING_DOCUMENT.WriteLine(Chr(13))
        SubSectionNumber += 1
        SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Justified)
        SETTING_DOCUMENT.SetParagraph(2) '
        SETTING_DOCUMENT.SetFont(BodyFont)
        SETTING_DOCUMENT.WriteLine("（3）穿刺风险评估：场地内砂层、粘土层交错分布，砂土层层厚较薄，尽管考虑了穿刺破坏模式验算，但并不能完全排除穿刺风险，实际施工中应特别谨慎操作，反复插拔、增加保压时间，防止液化、穿刺风险。")
        For Each Row In mydataset.Tables("LS_Boat").Select("IsCount=1")
            Dim RiskDrillingNames As String = ""
            For Each RDName In PunctureRiskDic(Row("ID"))
                RiskDrillingNames &= RDName & ","
            Next
            If RiskDrillingNames <> "" Then
                RiskDrillingNames = RiskDrillingNames.Remove(RiskDrillingNames.Length - 1, 1)
                SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Justified)
                SETTING_DOCUMENT.SetParagraph(2) '
                SETTING_DOCUMENT.SetFont(BodyFont)
                SETTING_DOCUMENT.WriteLine(Row("Name").ToString & "：需注意" & RiskDrillingNames & "等机位的穿刺风险。")
            End If
        Next
        SETTING_DOCUMENT.WriteLine(Chr(13))
        SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Justified)
        SETTING_DOCUMENT.SetParagraph(2) '
        SETTING_DOCUMENT.SetFont(BodyFont)
        SETTING_DOCUMENT.WriteLine("（4）本报告所采用的船舶相关参数由项目部提供，尤其是平台船的桩腿预压力和拔桩力对桩腿是否能够安全作业至关重要，因此项目部的桩腿插拔决策应结合船舶运营方管理要求、船舶操作手册中的船舶与装载信息、相关技术人员与操船人员的经验进行综合考虑。实际施工时须依据船舶操作手册以及相关规定进行施工，确保作业工况合规，保证平台稳性。
（5）地质资料方面：由于目前勘察资料主要服务于风机基础设计，其工况与自升式平台船插拔桩作业工况有较大差异，勘察资料提供的土质参数可能对软件计算结果造成一定偏差。此外，插拔桩作业桩靴实际停留位置与钻孔位置不尽相同，土层分布也会有所变化，也会对计算结果造成偏差。提供的勘察资料为中间成果报告可能会对计算结果造成一定偏差。
（6）当插深超过船舶已有施工经验时建议开展专题论证分析。
（7）自升式平台船桩靴插拔作业分析涉及较为复杂的岩土力学问题，对于插拔频繁的自升式风电安装船，国内外尚不存在能够完全准确预报桩腿插拔计算的模型，基于目前有限的相关研究和资料，计算假定破坏模式可能和实际的插拔桩破坏模式存在差别，这可能会导致计算结果的偏差。因而本报告仅供参考。")
    End Sub
    Sub GetResult(ByRef J As Integer, Row As DataRow, Irow As DataRow, N As Integer, TabColNames As String(), TabColShowNames As String(), IsPressResult As Boolean)
        J += 1
        For i = 0 To N - 1
            If TabColNames(i) <> "" Then
                Select Case TabColShowNames(i)
                    Case "持力层", "进入土层"
                        Table(J, i) = mydataset.Tables("LS_SoilDrillingParameter").Select("BoatID=" & Row("ID") & " and DrillingID=" & Irow("DrillingID") & " and ID=" & Irow(TabColNames(i)))(0)("Name")
                    Case "桩靴底部地基承载力（kPa）" '"最小承载力（小值）（kPa）","推荐参数承载力（中值）（kPa）", "最大承载力（大值）（kPa）"
                        Table(J, i) = Round(Irow(TabColNames(i)) / Row("SpudcanA"), 2)
                    Case "不计减阻系统的最大拔桩力（t）", "冲桩系统完全发挥作用时的拔桩力（t）"
                        Table(J, i) = Round(Irow(TabColNames(i)) / 9.8, 2)
                    Case "计算模式"
                        If IsPressResult Then
                            If Irow(TabColNames(i)) Then
                                Table(J, i) = mydataset.Tables("LS_ComputingModelType_Qv").Select("ID=" & Irow(TabColNames(i)))(0)("Name")
                            Else
                                If Irow("Qv") = Irow("Qv1") Then
                                    Table(J, i) = "常规破坏"
                                End If
                                If Irow("Qv") = Irow("Qv2") Then
                                    Table(J, i) = "挤出破坏"
                                End If
                                If Irow("Qv") = Irow("Qv3") Then
                                    Table(J, i) = "穿刺破坏"
                                End If
                            End If
                        Else
                            Table(J, i) = mydataset.Tables("LS_TempDeepType1").Select("ID=" & Irow(TabColNames(i)))(0)("Name")
                        End If
                    Case Else
                        Table(J, i) = Irow(TabColNames(i))
                End Select
            Else
                Select Case TabColShowNames(i)
                    Case "勘察孔"
                        Table(J, i) = mydataset.Tables("LS_TempSoilDrilling").Select("BoatID=" & Row("ID") & " and DrillingID=" & Irow("DrillingID"))(0)("DrillingName")
                    Case "平台船名"
                        Table(J, i) = Row("Name")
                    Case "泥面标高（m）"
                        Table(J, i) = mydataset.Tables("LS_SoilDrillingParameter").Compute("Max(TopLevel)", "BoatID=" & Row("ID") & " and DrillingID=" & Irow("DrillingID"))
                    Case "理论计算插深（m）", "插深（m）"
                        Table(J, i) = Table(J, i - 2) - Table(J, i - 1)
                End Select
            End If
        Next
    End Sub
    '页眉页脚
    Sub Setting_HeaderAndFooter()
        On Error Resume Next
        SETTING_DOCUMENT.HeaderStart()
        SETTING_DOCUMENT.SetFont(New Font(HeaderFont.FontFamily, HeaderFont.Size, FontStyle.Regular))
        SETTING_DOCUMENT.SetForegroundColor(HeaderColor)
        SETTING_DOCUMENT.SetTextAlign(HeaderAlign)
        SETTING_DOCUMENT.Write("自升式平台插拔桩计算软件 V1.0 计算报告书")
        SETTING_DOCUMENT.HeaderEnd()

        SETTING_DOCUMENT.FooterStart()
        SETTING_DOCUMENT.SetFont(New Font(FooterFont.FontFamily, FooterFont.Size))
        SETTING_DOCUMENT.SetForegroundColor(FooterColor)
        SETTING_DOCUMENT.SetTextAlign(FooterAlign)
        SETTING_DOCUMENT.Write("中交第三航务工程局有限公司")
        SETTING_DOCUMENT.SetPageNumbering(1)
        SETTING_DOCUMENT.FooterEnd()
    End Sub
    Sub Setting_Template_HeaderAndFooter(ByVal TaskName As String)
        On Error Resume Next
        SETTING_DOCUMENT.HeaderStart()
        SETTING_DOCUMENT.SetFont(New Font(HeaderFont.FontFamily, HeaderFont.Size, FontStyle.Regular))
        SETTING_DOCUMENT.SetForegroundColor(HeaderColor)
        SETTING_DOCUMENT.SetTextAlign(HeaderAlign)
        SETTING_DOCUMENT.Write(TaskName)
        SETTING_DOCUMENT.HeaderEnd()

        SETTING_DOCUMENT.FooterStart()
        SETTING_DOCUMENT.SetFont(New Font(FooterFont.FontFamily, FooterFont.Size))
        SETTING_DOCUMENT.SetForegroundColor(FooterColor)
        SETTING_DOCUMENT.SetTextAlign(FooterAlign)
        SETTING_DOCUMENT.Write(Format(Date.Now, "yyyy-MM-dd HH:mm:ss"))
        SETTING_DOCUMENT.SetPageNumbering(1)
        SETTING_DOCUMENT.FooterEnd()
    End Sub
    '封面
    Sub Setting_Cover()
        On Error Resume Next
        SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Center) '
        SETTING_DOCUMENT.WriteLine("")

        SETTING_DOCUMENT.SetFont(New Font("宋体", 12, FontStyle.Bold))
        SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Right)
        SETTING_DOCUMENT.Write(Chr(13) & "编号:______________")
        SETTING_DOCUMENT.SetFont(New Font("宋体", 14, (FontStyle.Regular)))
        SETTING_DOCUMENT.WriteLine("")
        SETTING_DOCUMENT.WriteLine("")
        SETTING_DOCUMENT.WriteLine("")
        SETTING_DOCUMENT.WriteLine("")
        SETTING_DOCUMENT.WriteLine("")
        SETTING_DOCUMENT.SetFont(New Font("宋体", 26, (FontStyle.Bold)))
        SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Center)
        SETTING_DOCUMENT.WriteLine("设 计 计 算 书")
        SETTING_DOCUMENT.SetFont(New Font("宋体", 14, (FontStyle.Regular)))
        SETTING_DOCUMENT.WriteLine("")
        SETTING_DOCUMENT.WriteLine("")
        SETTING_DOCUMENT.WriteLine("")
        SETTING_DOCUMENT.WriteLine("")
        SETTING_DOCUMENT.WriteLine("")
        SETTING_DOCUMENT.SetFont(New Font("宋体", 15, (FontStyle.Bold)))
        SETTING_DOCUMENT.Write(Chr(13) & "工程名称______________________________")
        SETTING_DOCUMENT.SetFont(New Font("宋体", 14, (FontStyle.Regular)))
        SETTING_DOCUMENT.WriteLine("")
        SETTING_DOCUMENT.WriteLine("")
        SETTING_DOCUMENT.SetFont(New Font("宋体", 15, (FontStyle.Bold)))
        SETTING_DOCUMENT.Write(Chr(13) & "设计阶段___________ 专业_____页数_____")
        SETTING_DOCUMENT.SetFont(New Font("宋体", 14, (FontStyle.Regular)))
        SETTING_DOCUMENT.WriteLine("")
        SETTING_DOCUMENT.WriteLine("")
        SETTING_DOCUMENT.SetFont(New Font("宋体", 15, (FontStyle.Bold)))
        SETTING_DOCUMENT.Write(Chr(13) & "计算书名称____________________________")
        SETTING_DOCUMENT.SetFont(New Font("宋体", 14, (FontStyle.Regular)))
        SETTING_DOCUMENT.WriteLine("")
        SETTING_DOCUMENT.WriteLine("")
        SETTING_DOCUMENT.WriteLine("")
        SETTING_DOCUMENT.WriteLine("")
        SETTING_DOCUMENT.WriteLine("")
        SETTING_DOCUMENT.WriteLine("")
        SETTING_DOCUMENT.WriteLine("")

        SETTING_DOCUMENT.SetFont(New Font("宋体", 14, (FontStyle.Bold)))
        SETTING_DOCUMENT.Write(Chr(13) & "计算:_______________日期_______________")
        SETTING_DOCUMENT.SetFont(New Font("宋体", 14, (FontStyle.Regular)))
        SETTING_DOCUMENT.WriteLine("")
        SETTING_DOCUMENT.WriteLine("")
        SETTING_DOCUMENT.SetFont(New Font("宋体", 14, (FontStyle.Bold)))
        SETTING_DOCUMENT.Write(Chr(13) & "校核:_______________日期_______________")
        SETTING_DOCUMENT.SetFont(New Font("宋体", 14, (FontStyle.Regular)))
        SETTING_DOCUMENT.WriteLine("")
        SETTING_DOCUMENT.WriteLine("")
        SETTING_DOCUMENT.SetFont(New Font("宋体", 14, (FontStyle.Bold)))
        SETTING_DOCUMENT.Write(Chr(13) & "审核:_______________日期_______________")
        SETTING_DOCUMENT.WriteLine("")
        SETTING_DOCUMENT.WriteLine("")
        SETTING_DOCUMENT.WriteLine("")
        SETTING_DOCUMENT.WriteLine("")
        SETTING_DOCUMENT.WriteLine("")
        SETTING_DOCUMENT.WriteLine("")
        SETTING_DOCUMENT.WriteLine("")
        SETTING_DOCUMENT.SetFont(New Font("宋体", 18, (FontStyle.Bold)))
        SETTING_DOCUMENT.WriteLine("中交第三航务工程局有限公司")
        SETTING_DOCUMENT.SetFont(New Font("宋体", 14, (FontStyle.Regular)))
        SETTING_DOCUMENT.WriteLine(Chr(13))
        SETTING_DOCUMENT.NewPage()
    End Sub
    '正文

    Sub Setting_Text_StructureData()
        On Error Resume Next
        ChapterNumber += 1
        SectionNumber = 0
        SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Center) '
        SETTING_DOCUMENT.SetFont(Heading1Font)
        SETTING_DOCUMENT.WriteLine("第" & GetChineseNumber() & "章 结构信息")
        Dim J As Integer
        Dim Irow As DataRow
        Dim Rows As DataRow()
        Dim TabName As String
        Dim TabColNumber As Integer
        Dim TabColNames As String()
        Dim TabColShowNames As String()
        '工程信息
        SectionNumber += 1
        SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Left)
        SETTING_DOCUMENT.SetFont(Heading2Font)
        SETTING_DOCUMENT.WriteLine(Chr(13) & ChapterNumber & "." & SectionNumber & " 工程信息")
        SETTING_DOCUMENT.SetFont(TableFont)
        SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Center)
        Irow = mydataset.Tables("LS_StructureData").Rows(0)
        ReDim Table(1, 8)
        Table(0, 0) = "计算人姓名"
        Table(0, 1) = "联系方式"
        Table(0, 2) = "风场名"
        Table(0, 3) = "船名"
        Table(0, 4) = "拔桩能力(t)"
        Table(0, 5) = "风场区域水深(m)"
        Table(0, 6) = "气隙(m)"
        Table(0, 7) = "冲桩系统是否具备"
        Table(0, 8) = "工作状态是否良好"
        ReDim TableWidths(8)
        For i = 0 To 8
            TableWidths(i) = 900
        Next
        TableWidths(0) = 1300
        TableWidths(1) = 1300
        'TableWidths(3) = 2000
        Table(1, 0) = Irow("UserName")
        Table(1, 1) = Irow("ContactNumber")
        Table(1, 2) = Irow("WindFieldName")
        Table(1, 3) = Irow("BoatName")
        Table(1, 4) = Irow("PullingCapacity")
        Table(1, 5) = Irow("WindFieldWaterHeight")
        Table(1, 6) = Irow("AirGap")
        Table(1, 7) = If(Irow("GetJettingSystem"), "是", "否"）
        Table(1, 8) = If(Irow("GoodWorking"), "是", "否"）
        Call inset_a_table(SETTING_DOCUMENT, Table, 9, 2, TableWidths)
        SETTING_DOCUMENT.WriteLine(Chr(13)) '回车
        '桩腿
        SectionNumber += 1
        SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Left)
        SETTING_DOCUMENT.SetFont(Heading2Font)
        SETTING_DOCUMENT.WriteLine(Chr(13) & ChapterNumber & "." & SectionNumber & " 桩腿")
        SETTING_DOCUMENT.SetFont(TableFont)
        SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Center)
        'ATable = New DataTable("LS_Leg")
        '.Add("ID", System.Type.GetType("System.Int32")) '采用自数据库时候ID
        '.Add("Type", System.Type.GetType("System.Int32")) '
        '.Add("Area", System.Type.GetType("System.Double")) '等效宽度
        '.Add("B", System.Type.GetType("System.Double")) '等效宽度
        '.Add("Circumference", System.Type.GetType("System.Double")) '等效长度
        '.Add("Diameter", System.Type.GetType("System.Double")) '等效直径
        '.Add("Volume", System.Type.GetType("System.Double")) '每延米体积
        '.Add("Weight", System.Type.GetType("System.Double")) '每延米重量(kN)
        '.Add("Parameter", System.Type.GetType("System.String")) '结构尺寸参数*****
        Irow = mydataset.Tables("LS_Leg").Rows(0)
        ReDim Table(2, 4)
        Table(0, 0) = "类型"
        Table(0, 1) = If（Irow("Type") = 1, "等效直径(m)", "桁架边长(m)")
        Table(0, 2) = "等效周长(m)"
        Table(0, 3) = "等效截面积(m{\super 2})"
        Table(0, 4) = "有效长度(m)"
        ReDim TableWidths(4)
        For i = 0 To 4
            TableWidths(i) = 1300
        Next
        'TableWidths(0) = 800
        'TableWidths(1) = 800
        'TableWidths(2) = 800
        'TableWidths(3) = 800
        Table(1, 0) = If(Irow("Type") = 1, "圆柱式", "桁架式")
        Table(1, 1) = Irow("Diameter")
        Table(1, 2) = Irow("Circumference")
        Table(1, 3) = Irow("Area")
        Table(1, 4) = Irow("ActiveLength")
        Call inset_a_table(SETTING_DOCUMENT, Table, 5, 2, TableWidths)
        SETTING_DOCUMENT.WriteLine(Chr(13)) '回车
        SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Justified)
        SETTING_DOCUMENT.SetFont(BodyFont)
        SETTING_DOCUMENT.WriteLine("*等效截面积用于计算回流土体体积。")
        '桩靴
        SectionNumber += 1
        SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Left)
        SETTING_DOCUMENT.SetFont(Heading2Font)
        SETTING_DOCUMENT.WriteLine(Chr(13) & ChapterNumber & "." & SectionNumber & " 桩靴")
        SETTING_DOCUMENT.SetFont(TableFont)
        SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Center)

        '.Add("ID", System.Type.GetType("System.Int32")) '采用自数据库时候ID
        '.Add("Type", System.Type.GetType("System.Int32")) '
        '.Add("ShapeType", System.Type.GetType("System.Int32")) '方形还是圆形
        '.Add("Ht", System.Type.GetType("System.Double")) '侧壁高度
        '.Add("B", System.Type.GetType("System.Double")) '等效宽度
        '.Add("L", System.Type.GetType("System.Double")) '等效长度
        '.Add("Circumference", System.Type.GetType("System.Double")) '最大处周长
        '.Add("Diameter", System.Type.GetType("System.Double")) '等效直径
        '.Add("Area", System.Type.GetType("System.Double")) '面积
        '.Add("Weight", System.Type.GetType("System.Double")) '重量(kN)
        '.Add("Volume", System.Type.GetType("System.Double")) '体积(m^3)
        '.Add("Parameter", System.Type.GetType("System.String")) '结构尺寸参数*****
        Dim SuInputType As Integer = mydataset.Tables("Ls_Common").Rows(0).Item("SuInputType")
        ReDim Table(2, 6)
        Table(0, 0) = "类型"
        Table(0, 1) = "等效直径(m)"
        Table(0, 2) = "周长(m)"
        Table(0, 3) = "面积(m{\super 2})"
        Table(0, 4) = "体积(m{\super 3})"
        Table(0, 5) = "(含桩腿)水下重量(kN)"
        Table(0, 6) = "几何参数"
        ReDim TableWidths(6)
        For i = 0 To 6
            TableWidths(i) = 1000
        Next
        TableWidths(6) = 1500
        'TableWidths(0) = 800
        'TableWidths(1) = 800
        'TableWidths(2) = 800
        'TableWidths(3) = 800
        'TableWidths(4) = 800
        'TableWidths(5) = 800
        'TableWidths(6) = 800
        'TableWidths(7) = 800
        Irow = mydataset.Tables("LS_Spudcan").Rows(0)
        Table(1, 0) = If(Irow("Type") = 1, "类四边形", "类圆形")
        Table(1, 1) = Irow("Diameter")
        Table(1, 2) = Irow("Circumference")
        Table(1, 3) = Irow("Area")
        Table(1, 4) = Irow("Volume")
        Table(1, 5) = Irow("Weight")
        Table(1, 6) = Irow("Parameter")
        Call inset_a_table(SETTING_DOCUMENT, Table, 7, 2, TableWidths)
        SETTING_DOCUMENT.WriteLine(Chr(13)) '回车

        '地层

        SectionNumber += 1
        SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Left)
        SETTING_DOCUMENT.SetFont(Heading2Font)
        SETTING_DOCUMENT.WriteLine(Chr(13) & ChapterNumber & "." & SectionNumber & " 土质物理指标")
        SETTING_DOCUMENT.SetFont(TableFont)
        SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Center)

        '.Add("ID", System.Type.GetType("System.Int32")) '土层ID
        '.Add("Name", System.Type.GetType("System.String")) '名称
        '.Add("Su", System.Type.GetType("System.String")) '不排水强度
        '.Add("UnderWaterWeight", System.Type.GetType("System.Double")) '水下重度
        '.Add("UnderWaterPhi", System.Type.GetType("System.Double")) '水下摩擦角
        '.Add("UnderWaterC", System.Type.GetType("System.Double")) '水下粘结力
        '.Add("E", System.Type.GetType("System.Double")) '弹性模量
        '.Add("mu", System.Type.GetType("System.Double")) '泊松比
        ReDim Table(mydataset.Tables("LS_Soil").Rows.Count, 7)
        Table(0, 0) = "名称"
        Table(0, 1) = "类型"
        Dim NColumn As Integer
        If SuInputType = 1 Then
            Table(0, 2) = "不排水抗剪强度Su0(kPa)"
            Table(0, 3) = "强度增长系数(kPa/m)"
            Table(0, 4) = "饱和重度kN/m{\super 3}"
            Table(0, 5) = "内摩擦角(°)"
            Table(0, 6) = "弹性模量(kN/m{\super 3})"
            Table(0, 7) = "泊松比"
            Table(0, 8) = "重度折减系数"
            Table(0, 9) = "强度折减系数"
            Table(0, 10) = "弹性模量折减系数"
            Table(0, 11) = "泊松比折减系数"

            NColumn = 12
        Else
            Table(0, 2) = "不排水抗剪强度(kPa)"
            Table(0, 3) = "饱和重度kN/m{\super 3}"
            Table(0, 4) = "水下摩擦角(°)"
            Table(0, 5) = "弹性模量(kN/m{\super 3})"
            Table(0, 6) = "泊松比"
            Table(0, 7) = "重度折减系数"
            Table(0, 8) = "强度折减系数"
            Table(0, 9) = "弹性模量折减系数"
            Table(0, 10) = "泊松比折减系数"

            NColumn = 11
        End If
        ReDim TableWidths(12)
        For i = 0 To 12
            TableWidths(i) = 900
        Next
        TableWidths(0) = 1500
        'TableWidths(1) = 800
        'TableWidths(2) = 800
        'TableWidths(3) = 800
        'TableWidths(4) = 800
        'TableWidths(5) = 800
        'TableWidths(6) = 800
        'TableWidths(7) = 800
        J = 0
        For Each Arow In mydataset.Tables("LS_Soil").Rows
            J += 1
            SoilNames.Add(Arow("ID"), Arow("Name"))
            Table(J, 0) = Arow("Name")
            Table(J, 1) = mydataset.Tables("LS_SoilType").Select("ID=" & Arow("Type"))(0)("Name")
            If SuInputType = 1 Then
                Table(J, 2) = Arow("Su0")
                Table(J, 3) = Arow("DSu")
                Table(J, 4) = Arow("UnderWaterWeight")
                Table(J, 5) = Arow("UnderWaterPhi")
                Table(J, 6) = Arow("E")
                Table(J, 7) = Arow("mu")
                Table(J, 8) = Arow("OnLegWeightReduceCoeff")
                Table(J, 9) = Arow("OnLegStrenthengReduceCoeff")
                Table(J, 10) = Arow("OnLegEReduceCoeff")
                Table(J, 11) = Arow("OnLegMuReduceCoeff")

            Else
                Table(J, 2) = Arow("Su")
                Table(J, 3) = Arow("UnderWaterWeight")
                Table(J, 4) = Arow("UnderWaterPhi")
                Table(J, 5) = Arow("E")
                Table(J, 6) = Arow("mu")
                Table(J, 7) = Arow("OnLegWeightReduceCoeff")
                Table(J, 8) = Arow("OnLegStrenthengReduceCoeff")
                Table(J, 9) = Arow("OnLegEReduceCoeff")
                Table(J, 10) = Arow("OnLegMuReduceCoeff")
            End If
        Next
        Call inset_a_table(SETTING_DOCUMENT, Table, NColumn, mydataset.Tables("LS_Soil").Rows.Count + 1, TableWidths)
        SETTING_DOCUMENT.WriteLine(Chr(13)) '回车
        SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Left)
        SETTING_DOCUMENT.SetFont(BodyFont)
        If SuInputType = 1 Then
            SETTING_DOCUMENT.WriteLine("注：Su0为该土层顶部不排水抗剪强度，Su从Su0开始按强度增长系数随高程线性变化。")
        Else
            SETTING_DOCUMENT.WriteLine("注：Su随高程线性变化。")
        End If

        SectionNumber += 1
        SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Left)
        SETTING_DOCUMENT.SetFont(Heading2Font)
        SETTING_DOCUMENT.WriteLine(Chr(13) & ChapterNumber & "." & SectionNumber & " 地层")
        SETTING_DOCUMENT.SetFont(TableFont)
        SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Center)

        Irow = mydataset.Tables("LS_Common").Rows(0)
        Dim UseSingleDrilling As Boolean = Irow("UseSingleDrilling")

        'EsDBTableSoilDrilling.Table("ID as [编号],Name as [钻孔名称],x as [钻孔" & Chr(13) & Chr(13) & "x(m)],y as [钻孔" & Chr(13) & Chr(13) & "y(m)],tempUI//地层//SoilLayers//地层:层顶高程(m)//120:80//地层:" & SoilName & ":" & SoilName & " as Table", "", "", "", "") = MyDataSet.Tables("LS_SoilDrilling")
        'Dbtable_PileSoilByDrilledSoil.Table("LS_Soil.Name As [土层名称],TopLevel As [层顶高程" & Chr(13) & Chr(13) & "(m)]", "LS_Soil", "LS_Soil.ID=LS_LegSoilLayer.SoilID", "", "") = MyDataSet.Tables("LS_LegSoilLayer")
        If UseSingleDrilling Then
            TabColShowNames = {"土层名称", "层顶高程(m)"}
            TabColNames = {"SoilID", "TopLevel"}
            Rows = mydataset.Tables("LS_LegSoilLayer").Select("")
        Else
            TabColShowNames = {"编号", "钻孔名称", "钻孔x(m)", "钻孔y(m)", "地层"}
            TabColNames = {"ID", "Name", "x", "y", "SoilLayers"}
            Rows = mydataset.Tables("LS_SoilDrilling").Select("")
        End If
        TabColNumber = TabColShowNames.Count
        ReDim Table(Rows.Count, TabColNumber - 1)
        ReDim TableWidths(TabColNumber - 1)
        For i = 0 To TabColNumber - 1
            Table(0, i) = TabColShowNames(i)
            TableWidths(i) = 1000
        Next
        If UseSingleDrilling Then
            TableWidths(0) = 2000
        Else
            TableWidths(0) = 600
            TableWidths(TabColNumber - 1) = 4000
        End If
        J = 0
        For Each Arow In Rows
            J += 1
            For i = 0 To TabColNumber - 1
                Table(J, i) = Arow(TabColNames(i))
                If TabColNames(i) = "SoilID" Then
                    Table(J, i) = SoilNames(Arow("SoilID"))
                End If
            Next
        Next
        Call inset_a_table(SETTING_DOCUMENT, Table, TabColNumber, J + 1, TableWidths)
        SETTING_DOCUMENT.WriteLine(Chr(13))
        SETTING_DOCUMENT.NewPage()

    End Sub
    Sub Setting_Text_CalculationParameter()
        On Error Resume Next
        ChapterNumber += 1
        SectionNumber = 0
        SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Center) '
        SETTING_DOCUMENT.SetFont(Heading1Font)
        SETTING_DOCUMENT.WriteLine("第" & GetChineseNumber() & "章 计算参数")

        Dim J As Integer
        Dim Irow As DataRow
        Dim TabName As String
        Dim TabColNumber As Integer
        Dim TabColNames As String()
        Dim TabColShowNames As String()
        SectionNumber += 1
        SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Left)
        SETTING_DOCUMENT.SetFont(Heading2Font)
        SETTING_DOCUMENT.WriteLine(Chr(13) & ChapterNumber & "." & SectionNumber & " 计算参数")
        SETTING_DOCUMENT.SetFont(TableFont)
        Irow = mydataset.Tables("LS_CalculationParameter").Rows(0) '竖向地基承载力计算参数
        '.Add("DestinationLevel", System.Type.GetType("System.Double")) '最大计算高程
        '.Add("NCalculatePoint", System.Type.GetType("System.Int32")) '计算高程点数量
        '.Add("CalculationMethod", System.Type.GetType("System.Int32")) '计算方法，公式法还是有限元法
        ''有限元法参数
        '.Add("MeshSize", System.Type.GetType("System.Double")) '计算单元尺寸
        '.Add("DPType", System.Type.GetType("System.Int32")) '使用的DP准则
        '.Add("KeepHistory", System.Type.GetType("System.Boolean")) '保留计算结果
        '.Add("DCoeff", System.Type.GetType("System.Double")) '系数收敛
        ''抗压
        '.Add("IsBackFlow", System.Type.GetType("System.Boolean")) '考虑回流
        ''抗拉
        '.Add("ftop", System.Type.GetType("System.Double")) '土体强度折减系数，桩靴上部土体因扰动产生的强度降低，与工作时间相关
        '.Add("fbase", System.Type.GetType("System.Double")) '强度增长系数，桩靴下部土体在荷载作用下再固结而产生强度增加，与工作时间相关
        '.Add("NBreakout", System.Type.GetType("System.Double")) '突破系数
        '.Add("SoilCoarseCoeff", System.Type.GetType("System.Double")) '粗糙度系数
        SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Left)
        SETTING_DOCUMENT.WriteLine("    计算方法：" & If(Irow("CalculationMethod") = 1, "BS EN ISO 19905-1:2016", "弹性塑性有限元法") & ";")
        SETTING_DOCUMENT.WriteLine("    计算桩靴底部高程(m)=" & Irow("DestinationLevel") & ";")
        SETTING_DOCUMENT.WriteLine("    计算高程点数量=" & Irow("NCalculatePoint") & ";")
        SETTING_DOCUMENT.WriteLine("    单腿预压力(t)=" & Irow("PressForce") & ";")
        If Irow("CalculationMethod") = 2 Then
            SETTING_DOCUMENT.WriteLine("    计算单元尺寸(m)=" & Irow("MeshSize") & ";")
            SETTING_DOCUMENT.WriteLine("    计算屈服准则：DP" & Irow("DPType") & ";")


        Else
            SETTING_DOCUMENT.WriteLine("    考虑土体回流：" & If(Irow("IsBackFlow"), "是", "否") & ";")
            SETTING_DOCUMENT.WriteLine("    自动计算极限孔洞深度Hc：" & If(Irow("AutoGetHc"), "是", "否") & ";")
            If Irow("AutoGetHc") = False Then SETTING_DOCUMENT.WriteLine("    Hc(m)=" & Irow("Hc") & ";")
            SETTING_DOCUMENT.WriteLine("    突破系数Nbreakout=" & Irow("NBreakout") & ";")
            SETTING_DOCUMENT.WriteLine("    桩土间粗糙度α=" & Irow("SoilCoarseCoeff") & ";")
            SETTING_DOCUMENT.WriteLine("    土体强度折减系数ftop=" & Irow("ftop") & "，桩靴上部土体因扰动产生的强度降低，与工作时间相关;")
            SETTING_DOCUMENT.WriteLine("    强度增长系数fbase=" & Irow("fbase") & ",桩靴下部土体在荷载作用下再固结而产生强度增加，与工作时间相关。")


        End If
        SETTING_DOCUMENT.NewPage()
    End Sub
    Sub Setting_Text_Result()
        On Error Resume Next
        ChapterNumber += 1
        SectionNumber = 0
        SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Center) '
        SETTING_DOCUMENT.SetFont(Heading1Font)
        SETTING_DOCUMENT.WriteLine("第" & GetChineseNumber() & "章 承载力曲线结果")
        Dim J As Integer
        Dim Irow As DataRow
        Dim TabName As String
        Dim TabColNumber As Integer
        Dim TabColNames As String()
        Dim TabColShowNames As String()
        Dim DrillingID As New List(Of Integer)
        Irow = mydataset.Tables("LS_CalculationParameter").Rows(0) '竖向地基承载力计算参数
        Dim CalculationMethod As Integer = Irow("CalculationMethod")
        Irow = mydataset.Tables("LS_Common").Rows(0)
        Dim UseSingleDrilling As Boolean = Irow("UseSingleDrilling")
        If UseSingleDrilling Then
            DrillingID.Add(1)
        Else
            For Each Arow In mydataset.Tables("LS_SoilDrilling").Select("", "ID")
                If DrillingID.Contains(Arow("ID")) = False Then DrillingID.Add(Arow("ID"))
            Next
        End If
        '计算结果简表
        Dim LimitValue1 As Double = mydataset.Tables("LS_CalculationParameter")(0)("GroundPressure")
        SectionNumber += 1
        SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Left)
        SETTING_DOCUMENT.SetFont(Heading2Font)
        SETTING_DOCUMENT.WriteLine(Chr(13) & ChapterNumber & "." & SectionNumber & " 计算结果简表")
        SETTING_DOCUMENT.SetFont(TableFont)
        TabColShowNames = {"钻孔号", "测试力(kN)", "建议插深(m)", "持力层", "持力层土强度参数", "抗拔力(kN)", "抗压力(kN)", "抗压承载力计算模式"}
        TabColNames = {"DrillingID", "LimitForce", "SuggestedDepth", "SupportSoilID", "SupportSoilStrength", "Qu", "Qv", "SelectMode_Qv"}

        TabColNumber = If(UseSingleDrilling, TabColShowNames.Count - 1, TabColShowNames.Count) - If(CalculationMethod = 1, 0, 1) '有限元法不考虑"抗压承载力计算模式"列
        ReDim Table(DrillingID.Count, TabColNumber - 1)
        ReDim TableWidths(TabColNumber - 1)
        For i = 0 To TabColNumber - 1
            Table(0, i) = If(UseSingleDrilling, TabColShowNames(i + 1), TabColShowNames(i))
            TableWidths(i) = 1000
        Next
        If UseSingleDrilling Then
            TableWidths(2) = 1300
        Else
            TableWidths(0) = 600
            TableWidths(3) = 1300
        End If
        If CalculationMethod = 1 Then TableWidths(TabColNumber - 1) = 1300

        Irow = mydataset.Tables("LS_StructureData").Rows(0)
        Dim AirGap As Double = Irow("AirGap")
        Dim WindFieldWaterHeight As Double = Irow("WindFieldWaterHeight")
        Irow = mydataset.Tables("LS_Leg").Rows(0)
        Dim LegActiveLength As Double = Irow("ActiveLength")
        Dim DepthOKString As String = ""
        Dim IsDepthOK As Boolean = True

        J = 0
        For Each Arow In mydataset.Tables("LS_DepthResult").Select("IsUserAdd=False", "DrillingID ASC")
            J += 1
            For i = 0 To TabColNumber - 1
                Dim TabCName As String = If(UseSingleDrilling, TabColNames(i + 1), TabColNames(i))
                Table(J, i) = Arow(TabCName)
                If TabCName = "SupportSoilStrength" Then
                    Table(J, i) = Arow(TabCName) & If(mydataset.Tables("LS_Soil").Select("ID=" & Arow("SupportSoilID"))(0)("Type") = 0, "kPa", "°")
                End If
                If TabCName = "SupportSoilID" Then
                    Table(J, i) = SoilNames(Table(J, i))
                End If
                If TabCName = "SelectMode_Qv" Then
                    Table(J, i) = mydataset.Tables("LS_ComputingModelType_Qv").Select("ID=" & Arow("SelectMode_Qv"))(0)("Name")
                End If
            Next
            DepthOKString &= If(UseSingleDrilling, "    ", "    钻孔#" & Arow("DrillingID")) & "建议插深结果为" & Arow("SuggestedDepth") & "m,插深、风场区域水深、气隙之和" & If(Arow("SuggestedDepth") + AirGap + WindFieldWaterHeight < LegActiveLength, "小于", "大于或等于") & "桩腿有效长度(" & LegActiveLength & "m)。" & vbLf
        Next
        Call inset_a_table(SETTING_DOCUMENT, Table, TabColNumber, J + 1, TableWidths)
        If mydataset.Tables("LS_DepthResult").Select("IsUserAdd=False", "DrillingID ASC").Count > 0 Then
            SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Left)
            SETTING_DOCUMENT.WriteLine(DepthOKString & "    注：默认计算模式抗压承载力取常规破坏模式、挤出模式和穿刺模式三种模式下的最小结果，其中常规破坏模式和穿刺破坏模式按砂土和黏土进行计算。")
        End If
        SETTING_DOCUMENT.WriteLine(Chr(13))

        '极限孔洞深度结果
        If CalculationMethod = 1 Then
            SectionNumber += 1
            SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Left)
            SETTING_DOCUMENT.SetFont(Heading2Font)
            SETTING_DOCUMENT.WriteLine(Chr(13) & ChapterNumber & "." & SectionNumber & " 极限孔洞深度结果")
            SETTING_DOCUMENT.SetFont(TableFont)
            TabColShowNames = {"钻孔号", "极限孔洞深度Hc(m)"}
            TabColNames = {"DrillingID", "Hc"}
            TabColNumber = If(UseSingleDrilling, TabColShowNames.Count - 1, TabColShowNames.Count)
            ReDim Table(DrillingID.Count, TabColNumber - 1)
            ReDim TableWidths(TabColNumber - 1)
            For i = 0 To TabColNumber - 1
                Table(0, i) = If(UseSingleDrilling, TabColShowNames(i + 1), TabColShowNames(i))
                TableWidths(i) = 2000
            Next
            If UseSingleDrilling Then
                TableWidths(0) = 1500
            Else
                TableWidths(0) = 1000
            End If
            J = 0
            For Each Arow In mydataset.Tables("LS_Holl").Select("", "DrillingID")
                J += 1
                For i = 0 To TabColNumber - 1
                    Table(J, i) = Arow(If(UseSingleDrilling, TabColNames(i + 1), TabColNames(i)))
                Next
            Next
            Call inset_a_table(SETTING_DOCUMENT, Table, TabColNumber, J + 1, TableWidths)
            SETTING_DOCUMENT.WriteLine(Chr(13))
        End If
        '承载力结果
        SectionNumber += 1
        SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Left)
        SETTING_DOCUMENT.SetFont(Heading2Font)
        SETTING_DOCUMENT.WriteLine(Chr(13) & ChapterNumber & "." & SectionNumber & " 地基承载力结果")
        SETTING_DOCUMENT.SetFont(TableFont)
        If CalculationMethod = 1 Then
            SETTING_DOCUMENT.WriteLine("    对于单一黏土层，若不排水抗剪强度不变或变化较小，常规破坏模式的地基极限竖向承载力按下式计算：")
            SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Center)
            SETTING_DOCUMENT.WriteLine("Q{\sub V}=(S{\sub u}N{\sub c}s{\sub c}d{\sub c}+p'{\sub 0})A")
            SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Left)
            SETTING_DOCUMENT.WriteLine("    对于单一均质砂土层，常规破坏模式的极限竖向承载力按下式计算：")
            SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Center)
            SETTING_DOCUMENT.WriteLine("Q{\sub V}=(γ'd{\sub γ}N{\sub γ}B/2+p'{\sub 0}d{\sub q}N{\sub q})A")
            SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Left)
            SETTING_DOCUMENT.WriteLine("    软黏土层厚度较小且下方存在硬土层时，应考虑挤出破坏，极限竖向承载力按下式计算：")
            SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Center)
            SETTING_DOCUMENT.WriteLine("Q{\sub V}=A{(α{\sub s}+b{\sub s}B/T+1.2D/B)S{\sub u}+p'{\sub 0}}")
            SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Left)
            SETTING_DOCUMENT.WriteLine("    当硬黏土层覆盖在软黏土层上时，应考虑穿刺破坏，极限竖向承载力按下式进行验算：")
            SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Center)
            SETTING_DOCUMENT.WriteLine("Q{\sub V}=A[3H/BS{\sub u,t}+N{\sub c}s{\sub c}(1+0.2(D+H)/B)S{\sub u,b}+p'{\sub 0})]")
            SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Left)
            SETTING_DOCUMENT.WriteLine("    当砂土层覆盖在软黏土层上时，应考虑穿刺破坏，极限竖向承载力可按下列公式进行计算：")
            SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Center)
            SETTING_DOCUMENT.WriteLine("Q{\sub V}=Q{\sub V,b}-AHγ'+2AH(Hγ'+2p'{\sub 0})K{\sub S}tan(φ'/B)")
            SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Left)
            SETTING_DOCUMENT.WriteLine("    考虑分层土承载力计算模式，极限竖向承载力可按下列公式进行计算：")
            SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Center)
            SETTING_DOCUMENT.WriteLine("Q{\sub V}=(0.5γ'BN{\sub γ}s{\sub γ}i{\sub γ}+p'{\sub 0}N{\sub q}s{\sub q}i{\sub q}+s{\sub u}N{\sub c}s{\sub c}i{\sub c})A")
            'SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Left)
            'SETTING_DOCUMENT.WriteLine("    注：最终极限抗压承载力取常规破坏模式、挤出模式和穿刺模式三种模式下的最小结果，其中常规破坏模式和穿刺破坏模式按砂土和黏土进行计算。")
            'If mydataset.Tables("LS_Holl").Rows.Count > 0 Then
            '    SETTING_DOCUMENT.WriteLine("    极限孔洞深度Hc(m)=" & mydataset.Tables("LS_Holl").Rows(0).Item("Hc") & "。")
            'End If
        Else
            SETTING_DOCUMENT.WriteLine("    注：极限抗压承载力和抗压承载力按照塑性有限元法计算，不断增加压力或拔力，当计算不稳定时即为临界荷载。")
        End If
        SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Center)

        'If CalculationMethod = 1 Then
        '    EsDBTablePressResult.Table("ID As [序号],Level as [底部高程(m)],Ls_Soil.Name As [进入土层],IsSand as [是否砂土],Qv as [地基极限竖向承载力Qv(kN)],Qv1 as [按照竖向承载模式Qv(kN)],Qv2 as [按照挤出模式计算Qv(kN)],Qv3 as [按照穿刺模式计算Qv(kN)]", "Ls_Soil", "Ls_Soil.ID=LS_PressResistanceResult.SoilID", "", "") = mydataset.Tables("LS_PressResistanceResult")
        'Else
        '    EsDBTablePressResult.Table("ID As [序号],Level as [底部高程(m)],Ls_Soil.Name As [进入土层],Qv as [地基极限竖向承载力Qv(kN)]", "Ls_Soil", "Ls_Soil.ID=LS_PressResistanceResult.SoilID", "", "") = mydataset.Tables("LS_PressResistanceResult")
        'End If
        'If CalculationMethod = 1 Then
        '    EsDBTablePullResult.Table("ID As [序号],Level as [底部高程(m)],Ls_Soil.Name As [进入土层],LS_DeepType.Name as [埋入模式],Qu as [拔桩力Qu(kN)]", "Ls_Soil,LS_DeepType", "Ls_Soil.ID=LS_PullResistanceResult.SoilID and LS_DeepType.ID=LS_PullResistanceResult.DeepType", "", "") = mydataset.Tables("LS_PullResistanceResult")
        'Else
        '    EsDBTablePullResult.Table("ID As [序号],Level as [底部高程(m)],Ls_Soil.Name As [进入土层],Qu as [拔桩力Qu(kN)]", "Ls_Soil", "Ls_Soil.ID=LS_PullResistanceResult.SoilID", "", "") = mydataset.Tables("LS_PullResistanceResult")
        'End If
        If CalculationMethod = 1 Then '公式法 
            TabColShowNames = {"钻孔号", "序号", "桩靴底部高程(m)", "进入土层", "是否进入砂土", "砂土常规破坏模式Qv(kN)", "黏土常规破坏模式Qv(kN)", "挤出破坏模式Qv(kN))", "砂土穿刺破坏模式Qv(kN)", "黏土穿刺破坏模式Qv(kN)", "分层土破坏模式Qv(kN)", "选择计算模式"} ', "地基承载力Qv(kPa)", "地基承载力Qv(kN)"
            TabColNames = {"DrillingID", "ID", "Level", "SoilID", "IsSand", "QV1_Sand", "QV1_Clay", "QV2", "QV3_Sand", "QV3_Clay", "QV4", "SelectMode"} ', "QVp", "QV"
            TabColNumber = If(UseSingleDrilling, TabColShowNames.Count - 1, TabColShowNames.Count)
            ReDim Table(mydataset.Tables("LS_PressResistanceResult").Rows.Count, TabColNumber - 1)
            ReDim TableWidths(TabColNumber - 1)
            For i = 0 To TabColNumber - 1
                Table(0, i) = If(UseSingleDrilling, TabColShowNames(i + 1), TabColShowNames(i))
                TableWidths(i) = 800
            Next
            TableWidths(0) = 600
            If UseSingleDrilling Then
                TableWidths(1) = 800
                TableWidths(3) = 600
            Else
                TableWidths(1) = 600
                TableWidths(2) = 800
                TableWidths(3) = 800
                TableWidths(4) = 600
            End If
            J = 0
            For Each Arow In mydataset.Tables("LS_PressResistanceResult").Rows
                J += 1
                For i = 0 To TabColNumber - 1
                    Dim ColName As String = If(UseSingleDrilling, TabColNames(i + 1), TabColNames(i))
                    Select Case ColName
                        Case "SoilID"
                            Table(J, i) = SoilNames(Arow(ColName))
                        Case "IsSand"
                            Table(J, i) = If(Arow(ColName), "是", "否")
                        Case "SelectMode"
                            Table(J, i) = mydataset.Tables("LS_ComputingModelType_Qv").Select("ID=" & Arow(ColName))(0)("Name")
                        Case Else
                            'Case "DrillingID", "ID", "Level"
                            Table(J, i) = Arow(ColName)
                            'Table(J, i) = If(Arow(ColName) = 10000000000, "-", Arow(ColName))
                    End Select
                Next
            Next

            'ReDim Table(mydataset.Tables("LS_PressResistanceResult").Rows.Count, 12)
            'Table(0, 0) = "序号"
            'Table(0, 1) = "桩靴底部高程(m)"
            'Table(0, 2) = "进入土层"
            'Table(0, 3) = "是否进入砂土"
            'Table(0, 4) = "砂土常规破坏模式Qv(kN)"
            'Table(0, 5) = "黏土常规破坏模式Qv(kN)"
            'Table(0, 6) = "挤出破坏模式Qv(kN))"
            'Table(0, 7) = "砂土穿刺破坏模式Qv(kN)"
            'Table(0, 8) = "黏土穿刺破坏模式Qv(kN)"
            'Table(0, 9) = "分层土破坏模式Qv(kN)"
            'Table(0, 10) = "选择计算模式"
            'Table(0, 11) = "地基承载力Qv(kPa)"
            'Table(0, 12) = "地基承载力Qv(kN)"

            'ReDim TableWidths(12)
            'TableWidths(0) = 300
            'TableWidths(1) = 1200
            'TableWidths(2) = 800
            'TableWidths(3) = 300
            'TableWidths(4) = 1200
            'TableWidths(5) = 1200
            'TableWidths(6) = 1200
            'TableWidths(7) = 1200
            'TableWidths(8) = 1200
            'TableWidths(9) = 1200
            'TableWidths(10) = 1200
            'TableWidths(11) = 1200
            'TableWidths(12) = 1200
            'J = 0
            'For Each Arow In mydataset.Tables("LS_PressResistanceResult").Rows
            '    J += 1
            '    Table(J, 0) = Irow("ID")
            '    Table(J, 1) = Irow("Level")
            '    Table(J, 2) = SoilNames(Irow("SoilID"))
            '    Table(J, 3) = If(Irow("IsSand"), "是", "否")
            '    Table(J, 4) = If(Irow("QV1_Sand") = 10000000000, "-", Irow("QV1_Sand"))
            '    Table(J, 5) = If(Irow("QV1_Clay") = 10000000000, "-", Irow("QV1_Clay"))
            '    Table(J, 6) = If(Irow("QV2") = 10000000000, "-", Irow("QV2"))
            '    Table(J, 7) = If(Irow("QV3_Sand") = 10000000000, "-", Irow("QV3_Sand"))
            '    Table(J, 8) = If(Irow("QV3_Clay") = 10000000000, "-", Irow("QV3_Clay"))
            '    Table(J, 9) = If(Irow("QV4") = 10000000000, "-", Irow("QV4"))
            '    Table(J, 10) = mydataset.Tables("LS_ComputingModelType_Qv").Select("ID=" & Irow("SelectMode"))(0)("Name")
            '    Table(J, 11) = If(Irow("QVp") = 10000000000, "-", Irow("QVp"))
            '    Table(J, 12) = If(Irow("QV") = 10000000000, "-", Irow("QV"))
            'Next
            'TabColNumber = 13
        Else
            TabColShowNames = {"钻孔号", "序号", "桩靴底部高程(m)", "地基承载力Qv(kPa)", "地基承载力Qv(kN)"}
            TabColNames = {"DrillingID", "ID", "Level", "QVp", "QV"}
            TabColNumber = If(UseSingleDrilling, TabColShowNames.Count - 1, TabColShowNames.Count)
            ReDim Table(mydataset.Tables("LS_PressResistanceResult").Rows.Count, TabColNumber - 1)
            ReDim TableWidths(TabColNumber - 1)
            For i = 0 To TabColNumber - 1
                Table(0, i) = If(UseSingleDrilling, TabColShowNames(i + 1), TabColShowNames(i))
                TableWidths(i) = 1200
            Next
            TableWidths(0) = 600
            If UseSingleDrilling = False Then TableWidths(1) = 600
            J = 0
            For Each Arow In mydataset.Tables("LS_PressResistanceResult").Rows
                J += 1
                For i = 0 To TabColNumber - 1
                    Dim ColName As String = If(UseSingleDrilling, TabColNames(i + 1), TabColNames(i))
                    Table(J, i) = Arow(ColName)
                    'Select Case ColName
                    '    Case "DrillingID", "ID", "Level"
                    '        Table(J, i) = Arow(ColName)
                    '    Case Else
                    '        Table(J, i) = If(Arow(ColName) = 10000000000, "-", Arow(ColName))
                    'End Select
                Next
            Next

            'ReDim Table(mydataset.Tables("LS_PressResistanceResult").Rows.Count, 7)
            'Table(0, 0) = "序号"
            'Table(0, 1) = "桩靴底部高程(m)"
            'Table(0, 2) = "地基承载力Qv'(kPa)"
            'Table(0, 3) = "地基承载力Qv'(kN)"
            'ReDim TableWidths(7)
            'TableWidths(0) = 300
            'TableWidths(1) = 1000
            'TableWidths(2) = 1000
            'TableWidths(3) = 1000
            'J = 0
            'For Each Arow In mydataset.Tables("LS_PressResistanceResult").Rows
            '    J += 1
            '    Table(J, 0) = Arow("ID")
            '    Table(J, 1) = Arow("Level")
            '    Table(J, 2) = If(Arow("QVp") = 10000000000, "-", Arow("QVp"))
            '    Table(J, 3) = If(Arow("QV") = 10000000000, "-", Arow("QV"))
            'Next
            'TabColNumber = 4
        End If

        Call inset_a_table(SETTING_DOCUMENT, Table, TabColNumber, J + 1, TableWidths)
        SETTING_DOCUMENT.WriteLine(Chr(13)) '回车

        Dim CurveTable As New EsPLCurveTable
        If J > 0 Then
            For Each DID In DrillingID
                Dim DName As String = If(UseSingleDrilling, "", mydataset.Tables("LS_SoilDrilling").Select("ID=" & DID)(0)("Name") & "-")
                CurveTable = New EsPLCurveTable
                CurveTable.Curves.Clear()
                'SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Center)
                SETTING_DOCUMENT.PutImage(SpudcanDB.DrawPressCurve(mydataset, CurveTable, LimitValue1, 1000, 500, 2, 1, DID), 600, 300)
                SETTING_DOCUMENT.WriteLine(Chr(13))
                SETTING_DOCUMENT.WriteLine(DName & "地基承载力曲线")
                SETTING_DOCUMENT.WriteLine(Chr(13))
            Next
        End If
        '拔桩力结果
        SectionNumber += 1
        SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Left)
        SETTING_DOCUMENT.SetFont(Heading2Font)
        SETTING_DOCUMENT.WriteLine(Chr(13) & ChapterNumber & "." & SectionNumber & " 拔桩力结果")
        SETTING_DOCUMENT.SetFont(TableFont)
        If CalculationMethod = 1 Then '公式法
            SETTING_DOCUMENT.WriteLine("    粘性土中，浅埋状态拔桩力可按下式计算：")
            SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Center)
            SETTING_DOCUMENT.WriteLine("Q{\sub breakout}=W+C(H{\sub column}S{\sub u}f{\sub top}+αH{\sub t}S{\sub u}f{\sub base})+A(N{\sub breakout}S{\sub u}f{\sub base}+H{\sub column}γ')-V{\sub top}γ'")
            SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Left)
            SETTING_DOCUMENT.WriteLine("    粘性土中，深埋状态时拔桩力可按下式计算：")
            SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Center)
            SETTING_DOCUMENT.WriteLine("Q{\sub breakout}=W+A(N{\sub breakout}S{\sub u}f{\sub base}+H{\sub column}γ')+A'S{\sub u}-V{\sub top}γ'")
            SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Left)
            SETTING_DOCUMENT.WriteLine("    砂性土中，浅埋状态拔桩力可按下式计算：")
            SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Center)
            SETTING_DOCUMENT.WriteLine("Q{\sub u}=2cD(B+L)+γD{\super 2}(2sB+L-B)K{\sub u}tanφ+W")
            SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Left)
            SETTING_DOCUMENT.WriteLine("    砂性土中，深埋状态时拔桩力可按下式计算：")
            SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Center)
            SETTING_DOCUMENT.WriteLine("Q{\sub u}=2cD(B+L)+γ(2D-H)H(2sB+L-B)K{\sub u}tanφ+W")
            SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Left)
            SETTING_DOCUMENT.WriteLine(Chr(13))
            'If CalculationMethod = 1 Then SETTING_DOCUMENT.WriteLine("    极限孔洞深度Hc(m)=" & mydataset.Tables("LS_Holl").Rows(0).Item("Hc") & "。")
            SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Center)

            TabColShowNames = {"钻孔号", "序号", "桩靴底部高程(m)", "进入土层", "砂土拔桩力Qu(kN)", "砂土插深类型", "黏土拔桩力Qu(kN)", "黏土插深类型", "选择计算模式"} ', "拔桩力Qu(t)", "拔桩力Qu(kN)", "插深类型"
            TabColNames = {"DrillingID", "ID", "Level", "SoilID", "Qu_Sand", "DeepType_Sand", "Qu_Clay", "DeepType_Clay", "SelectMode"} ', "QuP", "Qu", "DeepType"
            TabColNumber = If(UseSingleDrilling, TabColShowNames.Count - 1, TabColShowNames.Count)
            ReDim Table(mydataset.Tables("LS_PullResistanceResult").Rows.Count, TabColNumber - 1)
            ReDim TableWidths(TabColNumber - 1)
            For i = 0 To TabColNumber - 1
                Table(0, i) = If(UseSingleDrilling, TabColShowNames(i + 1), TabColShowNames(i))
                TableWidths(i) = 1000
            Next
            TableWidths(0) = 600
            If UseSingleDrilling Then
                TableWidths(1) = 800
                TableWidths(4) = 600
                TableWidths(6) = 600
                TableWidths(7) = 800
                TableWidths(10) = 600
            Else
                TableWidths(1) = 600
                TableWidths(2) = 800
                TableWidths(5) = 600
                TableWidths(7) = 600
                TableWidths(7) = 800
                TableWidths(11) = 600
            End If
            J = 0
            For Each Arow In mydataset.Tables("LS_PullResistanceResult").Rows
                J += 1
                For i = 0 To TabColNumber - 1
                    Dim ColName As String = If(UseSingleDrilling, TabColNames(i + 1), TabColNames(i))
                    Select Case ColName
                        Case "SoilID"
                            Table(J, i) = SoilNames(Arow(ColName))
                        'Case "IsSand"
                        '    Table(J, i) = If(Arow(ColName), "是", "否")
                        Case "DeepType_Sand", "DeepType_Clay", "DeepType"
                            Table(J, i) = mydataset.Tables("LS_DeepType").Select("ID=" & Arow(ColName))(0)("Name")
                        Case "SelectMode"
                            Table(J, i) = mydataset.Tables("LS_ComputingModelType_Qb").Select("ID=" & Arow(ColName))(0)("Name")
                        Case Else
                            'Case "DrillingID", "ID", "Level"
                            Table(J, i) = Arow(ColName)
                            'Table(J, i) = If(Arow(ColName) = 10000000000, "-", Arow(ColName))
                    End Select
                Next
            Next

            'ReDim Table(mydataset.Tables("LS_PullResistanceResult").Rows.Count, 10)
            'Table(0, 0) = "序号"
            'Table(0, 1) = "桩靴底部高程(m)"
            'Table(0, 2) = "进入土层"
            'Table(0, 3) = "砂土拔桩力Qu(kN)"
            'Table(0, 4) = "砂土插深类型"
            'Table(0, 5) = "黏土拔桩力Qu(kN)"
            'Table(0, 6) = "黏土插深类型"
            'Table(0, 7) = "选择计算模式"
            'Table(0, 8) = "拔桩力Qu(t)"
            'Table(0, 9) = "拔桩力Qu(kN)"
            'Table(0, 10) = "插深类型"

            'ReDim TableWidths(10)
            'TableWidths(0) = 300
            'TableWidths(1) = 800
            'TableWidths(2) = 1200
            'TableWidths(3) = 1200
            'TableWidths(4) = 1200
            'TableWidths(5) = 1200
            'TableWidths(6) = 1200
            'TableWidths(7) = 1200
            'TableWidths(8) = 1200
            'TableWidths(9) = 1200
            'TableWidths(10) = 1200

            'J = 0
            'For Each Arow In mydataset.Tables("LS_PullResistanceResult").Rows
            '    J += 1
            '    Table(J, 0) = Irow("ID")
            '    Table(J, 1) = Irow("Level")
            '    Table(J, 2) = SoilNames(Irow("SoilID"))
            '    Table(J, 3) = If(Irow("Qu_Sand") = 10000000000, "-", Irow("Qu_Sand"))
            '    Table(J, 4) = mydataset.Tables("LS_DeepType").Select("ID=" & Irow("DeepType_Sand"))(0)("Name")
            '    Table(J, 5) = If(Irow("Qu_Clay") = 10000000000, "-", Irow("Qu_Clay"))
            '    Table(J, 6) = mydataset.Tables("LS_DeepType").Select("ID=" & Irow("DeepType_Clay"))(0)("Name")
            '    Table(J, 7) = mydataset.Tables("LS_ComputingModelType_Qb").Select("ID=" & Irow("SelectMode"))(0)("Name")
            '    Table(J, 8) = If(Irow("QuP") = 10000000000, "-", Irow("QuP"))
            '    Table(J, 9) = If(Irow("Qu") = 10000000000, "-", Irow("Qu"))
            '    Table(J, 10) = mydataset.Tables("LS_DeepType").Select("ID=" & Irow("DeepType"))(0)("Name")
            'Next
            'TabColNumber = 11
        Else
            SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Center)
            TabColShowNames = {"钻孔号", "序号", "桩靴底部高程(m)", "拔桩力Qu(t)", "拔桩力Qu(kN)"}
            TabColNames = {"DrillingID", "ID", "Level", "QuP", "Qu"}
            TabColNumber = If(UseSingleDrilling, TabColShowNames.Count - 1, TabColShowNames.Count)
            ReDim Table(mydataset.Tables("LS_PullResistanceResult").Rows.Count, TabColNumber - 1)
            ReDim TableWidths(TabColNumber - 1)
            For i = 0 To TabColNumber - 1
                Table(0, i) = If(UseSingleDrilling, TabColShowNames(i + 1), TabColShowNames(i))
                TableWidths(i) = 1200
            Next
            TableWidths(0) = 600
            If UseSingleDrilling = False Then TableWidths(1) = 600
            J = 0
            For Each Arow In mydataset.Tables("LS_PullResistanceResult").Rows
                J += 1
                For i = 0 To TabColNumber - 1
                    Dim ColName As String = If(UseSingleDrilling, TabColNames(i + 1), TabColNames(i))
                    Table(J, i) = Arow(ColName)
                    'Select Case ColName
                    '    Case "DrillingID", "ID", "Level"
                    '        Table(J, i) = Arow(ColName)
                    '    Case Else
                    '        Table(J, i) = If(Arow(ColName) = 10000000000, "-", Arow(ColName))
                    'End Select
                Next
            Next

            'ReDim Table(mydataset.Tables("LS_PullResistanceResult").Rows.Count, 7)
            'Table(0, 0) = "序号"
            'Table(0, 1) = "桩靴底部高程(m)"
            'Table(0, 2) = "拔桩力Qu(kN)"
            'ReDim TableWidths(7)
            'TableWidths(0) = 300
            'TableWidths(1) = 1200
            'TableWidths(2) = 1200
            'J = 0
            'For Each Arow In mydataset.Tables("LS_PullResistanceResult").Rows
            '    J += 1
            '    Table(J, 0) = Arow("ID")
            '    Table(J, 1) = Arow("Level")
            '    Table(J, 2) = Arow("Qu")
            'Next
            'TabColNumber = 3
        End If
        Call inset_a_table(SETTING_DOCUMENT, Table, TabColNumber, J + 1, TableWidths)
        If J > 0 Then
            For Each DID In DrillingID
                Dim DName As String = If(UseSingleDrilling, "", mydataset.Tables("LS_SoilDrilling").Select("ID=" & DID)(0)("Name"))
                CurveTable = New EsPLCurveTable
                CurveTable.Curves.Clear()
                'SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Center)
                SETTING_DOCUMENT.PutImage(SpudcanDB.DrawPullCurve(mydataset, CurveTable, 1000, 500, 2, 1, DID), 600, 300)
                SETTING_DOCUMENT.WriteLine(Chr(13))
                SETTING_DOCUMENT.WriteLine(DName & "拔桩力曲线")
                SETTING_DOCUMENT.WriteLine(Chr(13))
            Next
        End If
        'SETTING_DOCUMENT.NewPage()
    End Sub
    Sub Setting_Text_FiniteElementResult()
        On Error Resume Next
        ChapterNumber += 1
        SectionNumber = 0
        SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Center) '
        SETTING_DOCUMENT.SetFont(Heading1Font)
        SETTING_DOCUMENT.WriteLine("第" & GetChineseNumber() & "章 有限元计算结果")
        '承载力结果
        Dim J As Integer
        Dim Irow As DataRow
        Dim TabName As String
        Dim TabColNumber As Integer
        Dim TabColNames As String()
        Dim TabColShowNames As String()
        SectionNumber += 1
        SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Left)
        SETTING_DOCUMENT.SetFont(Heading2Font)
        SETTING_DOCUMENT.WriteLine(Chr(13) & ChapterNumber & "." & SectionNumber & " 有限元模型结果")
        SETTING_DOCUMENT.SetFont(TableFont)
        SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Center)

        SectionNumber += 1
        SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Left)
        SETTING_DOCUMENT.SetFont(Heading2Font)
        SETTING_DOCUMENT.WriteLine(Chr(13) & ChapterNumber & "." & SectionNumber & " 有限元塑性应变与承载力结果")
        SETTING_DOCUMENT.SetFont(TableFont)
        SETTING_DOCUMENT.SetTextAlign(EsWordTextAlign.Center)
    End Sub

    Function GetChineseNumber() As String
        Dim ChineseNumber() As String = {"一", "二", "三", "四", "五", "六", "七", "八", "九", "十"}
        Return ChineseNumber(ChapterNumber - 1)
    End Function
    Sub GetTableAndWidths(ByRef Table(,) As String, ByRef TabColNumber As Integer, ByRef J As Integer, ByVal TabName As String, ByVal TabColNames As String(), ByVal TabColShowNames As String(), ByVal filterExpression As String, ByVal sort As String, Optional ColWidth As Integer = 1300)
        J = 0
        TabColNumber = TabColNames.Count - 1
        ReDim Table(mydataset.Tables(TabName).Rows.Count, TabColNumber)
        ReDim TableWidths(TabColNumber)
        For i = 0 To TabColNumber
            Table(J, i) = TabColShowNames(i)
            TableWidths(i) = ColWidth
        Next
        For Each Arow In mydataset.Tables(TabName).Select(filterExpression, sort)
            J += 1
            For i = 0 To TabColNumber
                If TabColNames(i).Contains(",") Then
                    Dim TempS As String() = TabColNames(i).Split(",")
                    Table(J, i) = mydataset.Tables(TempS(0)).Select(TempS(1) & "=" & Arow(TempS(2)))(0)(TempS(3))
                Else
                    Table(J, i) = Arow(TabColNames(i))
                End If
            Next
        Next
        'Irow = mydataset.Tables(TabName).Rows(0)
        'J += 1
    End Sub
    Sub inset_a_table(ByRef RTB As EsWordDocument, ByVal table(,) As String, ByVal cols As Integer, ByVal rows As Integer, ByVal TableWidths() As Long, Optional ByVal NMergeColumn As Integer = 0, Optional ByVal MergeColumnIndex() As Integer = Nothing)
        Dim regular As New Font("Helvetica", 10, FontStyle.Regular)
        Dim rt As EsWordTable
        Dim I As Integer, J As Integer, Index As Integer, StartIndex As Integer, EndIndex As Integer
        Dim W As Integer = 0, Irow As Integer
        Dim m As Integer = 0, StartRow As Integer, RowIndex As Integer
        Dim ColumnIsMerge(cols) As Boolean, NMergeTable(cols) As Integer, MergeTable(rows, cols, 2) As Integer

        '查看合并行列
        For I = 0 To cols - 1
            ColumnIsMerge(I) = False
            NMergeTable(I) = 0
        Next
        If NMergeColumn <> 0 Then
            For I = 0 To NMergeColumn - 1
                ColumnIsMerge(MergeColumnIndex(I)) = True
                StartRow = 0
                RowIndex = 0
                For J = 0 To rows - 1
                    If table(J, MergeColumnIndex(I)) <> table(StartRow, MergeColumnIndex(I)) Then
                        MergeTable(RowIndex, I, 0) = StartRow
                        MergeTable(RowIndex, I, 1) = J - StartRow
                        RowIndex += 1
                        StartRow = J
                    End If
                Next
                MergeTable(RowIndex, I, 0) = StartRow
                MergeTable(RowIndex, I, 1) = rows - StartRow
                NMergeTable(MergeColumnIndex(I)) = RowIndex
            Next I
        End If
        For J = 0 To cols - 1
            W += TableWidths(J)
        Next
        '  start判断w（列的宽度）是否大于A4宽度，并求出不超出A4的列的最大数值zj
        If W > 9000 Then
            StartIndex = 0
            EndIndex = 0
            Do While EndIndex <> cols - 1
                W = 0
                EndIndex = 0
                For J = StartIndex To cols - 1
                    W += TableWidths(J)
                    If W > 9000 Then
                        If (cols - 1 - J) < 2 Then
                            EndIndex = J - 1
                        Else
                            EndIndex = J - 1
                        End If
                        Exit For

                    End If
                Next
                If EndIndex = 0 Then EndIndex = cols - 1
                rt = RTB.NewTable(regular, Color.Black, rows, EndIndex - StartIndex + 1, 0)
                rt.Alignment = EsWordTextAlign.Center
                rt.SetBorders(Color.Black, 2, True, True, True, True) '

                For J = StartIndex To EndIndex
                    rt.Columns(J - StartIndex).SetWidth(TableWidths(J))

                Next

                For J = StartIndex To EndIndex
                    If NMergeColumn = 0 Or ColumnIsMerge(J) = False Then
                        For I = 0 To rows - 1
                            'rt.Rows(I)(J - StartIndex).SetBorders(Color.Black, 2, True, True, True, True)
                            rt.Rows(I)(J - StartIndex).Write(table(I, J))
                            'rt.SetBorders(Color.Black, 2, True, True, True, True) '
                        Next
                    Else
                        For I = 0 To NMergeTable(J)
                            Irow = MergeTable(I, J, 0)
                            'rt.Rows(Irow)(J - StartIndex).SetBorders(Color.Black, 2, True, True, True, True)
                            rt.Rows(Irow)(J - StartIndex).Write(table(Irow, J))
                            rt.Rows(Irow)(J - StartIndex).RowSpan = MergeTable(I, J, 1)
                            rt.SetBorders(Color.Black, 2, True, True, True, True) '
                        Next
                    End If
                Next
                If Index > 0 Then
                    Dim bold1 As New Font("Tahoma", 10, FontStyle.Bold)
                    RTB.SetFont(bold1)
                    RTB.WriteLine("续表：")
                End If
                rt.SaveToDocument(W, 0)
                StartIndex = EndIndex + 1
                Index += 1
            Loop
        Else
            rt = RTB.NewTable(regular, Color.Black, rows, cols, 0)
            rt.Alignment = EsWordTextAlign.Center '表格居中
            rt.SetBorders(Color.Black, 2, True, True, True, True) '

            '正常输出表格数据
            For J = 0 To cols - 1
                rt.Columns(J).SetWidth(TableWidths(J))
                If NMergeColumn = 0 Or ColumnIsMerge(J) = False Then
                    For I = 0 To rows - 1
                        rt.Rows(I)(J).Write(table(I, J))
                    Next
                Else
                    For I = 0 To NMergeTable(J)
                        Irow = MergeTable(I, J, 0)
                        rt.Rows(Irow)(J).Write(table(Irow, J))
                        rt.Rows(Irow)(J).RowSpan = MergeTable(I, J, 1)
                        rt.SetBorders(Color.Black, 2, True, True, True, True) '
                    Next
                End If
            Next

            rt.SaveToDocument(W, 0)
        End If

    End Sub
End Class
