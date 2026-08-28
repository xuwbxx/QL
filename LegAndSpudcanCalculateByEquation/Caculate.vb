Imports System.Data
Imports System.Math
Imports Easy
'Public Class ReadME
'    Shared Sub Test()
'        Dim LSDataSet As New DataSet
'        Dim ASpudcanDB As New SpudcanDB(LSDataSet, )
'        Dim BoatAppDic As Dictionary(Of Integer, DataSet) = ASpudcanDB.GetBoatAppDic()
'        Dim SpudcanCaculate As New SpudcanCaculate(BoatAppDic(BoatAppDic.Keys(0)))
'        Dim ErrorString As String = ""
'        SpudcanCaculate.ComputeLevels(ErrorString)
'        If ErrorString = "" Then
'            SpudcanCaculate.WriteResult(BoatAppDic.Keys(0), LSDataSet)
'        End If
'    End Sub
'End Class
Public Class SoilParameter
    Public Name As String
    Public SoilID As Integer
    Public SoilType As SoilType
    Public SuCurve As EsValue2s
    Public Su0 As Double
    Public DSu As Double
    Public Phi As Double
    Public Weight As Double
    Public TopLevel As Double
    Public BottomLevel As Double
    Public SuInputType As Integer
    Public OnLegWeightReduceCoeff As Double
    Public OnLegStrenthengReduceCoeff As Double
    Public OnLegEReduceCoeff As Double
    Public OnLegMuReduceCoeff As Double


    Function GetCopy() As SoilParameter
        Dim ASoilParameter As New SoilParameter
        ASoilParameter.SoilID = SoilID
        ASoilParameter.SoilType = SoilType
        ASoilParameter.SuCurve = SuCurve
        ASoilParameter.Su0 = Su0 =
        ASoilParameter.DSu = DSu
        ASoilParameter.Phi = Phi
        ASoilParameter.Weight = Weight
        ASoilParameter.TopLevel = TopLevel
        ASoilParameter.BottomLevel = BottomLevel
        ASoilParameter.SuInputType = SuInputType

        ASoilParameter.OnLegWeightReduceCoeff = OnLegWeightReduceCoeff
        ASoilParameter.OnLegStrenthengReduceCoeff = OnLegStrenthengReduceCoeff
        ASoilParameter.OnLegEReduceCoeff = OnLegEReduceCoeff
        ASoilParameter.OnLegMuReduceCoeff = OnLegMuReduceCoeff
        ASoilParameter.Name = Name

        Return ASoilParameter
    End Function
    Sub New()
        SoilType = SoilType.Clay
        SuCurve = New EsValue2s
        Phi = 0
        TopLevel = 0
        BottomLevel = 0
        SuInputType = 1
        Name = ""
    End Sub
    Function GetSu(ByVal Level As Double) As Double
        If SuInputType = 1 Then
            Return Su0 + DSu * (TopLevel - Level)
        Else
            Return SuCurve.GetValue(Level)
        End If
    End Function
    Function GetSu0() As Double
        If SuInputType = 1 Then
            Return Su0
        Else
            If SuCurve.Values.Count > 0 Then
                Return SuCurve.Values(0).V2
            Else
                Return 0
            End If
        End If
    End Function

    Function GetDSu() As Double
        If SuInputType = 1 Then
            Return DSu
        Else
            If SuCurve.Values.Count > 1 Then
                Return (SuCurve.Values(SuCurve.Values.Count - 1).V2 - SuCurve.Values(0).V2) / (SuCurve.Values(SuCurve.Values.Count - 1).V1 - SuCurve.Values(0).V1)
            Else
                Return 0
            End If
        End If
    End Function

End Class
Public Enum SoilType
    Clay = 0
    Sand = 1
    Both = 2
End Enum
Public Class LegParameter
    Public Type As Integer
    Public Circumference As Double
    Public Diameter As Double
    Public Area As Double
    'Public Weight As Double
    'Public Volume0 As Double
    'Public Volume As Double
    'Public TopLevel As Double
    Sub New()
        Circumference = 0
        Diameter = 0
        Area = 0
        'Weight = 0
        'Volume = 0
        'Volume0 = 0
        'TopLevel = 0
    End Sub
End Class
Public Class SpudcanParameter
    Public Type As Integer
    Public ShapeType As Integer '0 为圆形，1为方形
    Public Circumference As Double
    Public Diameter As Double
    Public Weight As Double
    Public Area As Double
    Public L As Double
    Public B As Double
    Public Volume As Double
    Public Ht As Double
    Public B1 As Double
    Public H1 As Double
    Public H2 As Double
    Public H3 As Double
    Public H4 As Double
    Public L1 As Double
    Public L2 As Double
    Sub New()
        Circumference = 0
        Diameter = 0
        Weight = 0
        Area = 0
        L = 0
        Volume = 0
        Ht = 0
    End Sub
    Function GetSpudcanB() As Double
        Return If(ShapeType = 0, Diameter, Min(L, B))
    End Function
    Function GetVd()
        'VD--桩靴与土体接触部分的最大承载截面以下的桩靴体积
        Dim Vd As Double
        Dim A, ADown, VDown As Double
        Select Case ShapeType
            Case 0
                A = PI * (B / 2) ^ 2
                ADown = PI * (L2 / 2) ^ 2
            Case 1
                A = B * L
                ADown = A * (L2 / L) ^ 2
        End Select
        If ADown = 0 Then
            VDown = 0
        Else
            VDown = H2 * (ADown + A + (ADown * A) ^ 0.5) / 3
        End If
        Vd = VDown
        Return Vd
    End Function
End Class
Public Class CalculateParameter
    Public DestinationLevel As Double
    Public NCalculatePoint As Integer
    Public CalculationMethod As Integer
    Public MeshSize As Double
    Public DPType As Integer
    Public KeepHistory As Boolean
    Public DCoeff As Double
    Public IsBackFlow As Boolean
    Public AutoGetHc As Boolean
    Public ftop As Double
    Public fbase As Double
    Public fleg As Double
    Public NBreakout As Double
    Public alpha As Double
    Public Hc As Double
    Public Hc2 As Double
    Public CaculateL As Double
    Public CaculateH As Double
    Public fb As Double '冲桩减阻系数fb
    Public IsEquivalentToCircleSpudcan As Boolean '是否等效为圆形桩靴，针对砂土
    Public UnderWaterPhiSubtractValue As Double '砂土内摩擦角降低度数
    Public PressForce As Double '计算预压荷载(t)，为桩腿预压力与桩腿、桩靴自重之和，同LS_Boat中的SumW
    Sub New()
        UnderWaterPhiSubtractValue = 5
        IsEquivalentToCircleSpudcan = 1
        DestinationLevel = 0
        NCalculatePoint = 0
        CalculationMethod = 0
        MeshSize = 0
        DPType = 0
        KeepHistory = True
        DCoeff = 0
        IsBackFlow = False
        AutoGetHc = False
        ftop = 1
        fbase = 1
        fleg = 0
        NBreakout = 1
        alpha = 1
        Hc = 0
        Hc2 = 0
        CaculateL = 40
        CaculateH = 20
        fb = 1
        PressForce = 4500


    End Sub
End Class
Public Class SpudcanCaculate
    Public MyDataSet As DataSet
    Private WarningMessageList As List(Of String)
    Private ErrorMessageList As List(Of String)
    Private BoatAppDic As Dictionary(Of Integer, DataSet)
    Private BoatID As Integer
    Sub New()
        '根据易工提供的初始化代码改变2026-8-19
        Dim LSDataSet As New DataSet
        Dim ASpudcanDB As New SpudcanDB(LSDataSet, True)
        BoatAppDic = ASpudcanDB.GetBoatAppDic()
        BoatID = BoatAppDic.Keys(0)
        MyDataSet = BoatAppDic(BoatID)

        WarningMessageList = New List(Of String)
        ErrorMessageList = New List(Of String)
        AddHandler EsMessageReporter.ReportMessage, AddressOf ReportMessage
    End Sub
    Sub ReportMessage(ByVal Message As String, ByVal MessageType As EsMessageType)
        If MessageType = EsMessageType.Warning Then
            If Not WarningMessageList.Contains(Message) Then
                WarningMessageList.Add(Message)
            End If
        End If
        If MessageType = EsMessageType.Error Then
            If Not ErrorMessageList.Contains(Message) Then
                ErrorMessageList.Add(Message)
            End If
        End If
    End Sub
    Public Function GetWarningAndErrorMessage() As String
        Dim Message As String = ""
        If WarningMessageList.Count Then
            Message &= "警告信息："
            For Each WM In WarningMessageList
                Message &= WM
            Next
        End If
        If ErrorMessageList.Count Then
            Message &= "错误信息："
            For Each WM In ErrorMessageList
                Message &= WM
            Next
        End If
        If Message <> "" Then
            Message = Message.Replace(vbCrLf, ";" & vbCr)
            Message = Message.Remove(Message.Length - 2, 2) 'vbCr,;2个字符
            Message &= "。"
        End If
        Return Message
    End Function
    Sub WriteResult(ByVal BoatID As Integer, ADataSet As DataSet)
        Dim BoatsDataSet As DataSet = ADataSet
        'Dim MyDataSet As DataSet = GetData
        Dim ABoatDataSet As DataSet = Me.MyDataSet
        'Dim TabNames As String() = {"LS_Holl", "LS_PressResistanceResult", "LS_PullResistanceResult", "LS_DepthResult", "LS_Load", "Ls_ResultOfNodeDisplacement", "LS_ResultOfFace",
        '    "LS_CalculationMaterials", "LS_CalculationLevels", "LS_CalculationNodes", "LS_CalculationEdges", "LS_CalculationAreas", "LS_MeshNodes", "LS_AreaMeshs", "LS_InfiniteMeshs", "LS_CoupleNodes", "LS_Contactors"}

        Dim NotResultTabNames As String() = SpudcanDB.GetNotResultTabNames()
        For Each ATable As DataTable In BoatsDataSet.Tables
            If Not NotResultTabNames.Contains(ATable.TableName) And ATable.TableName.Contains("LS_") Then
                For Each row In ATable.Select("BoatID=" & BoatID)
                    ATable.Rows.Remove(row)
                Next
                Dim TheTable As DataTable = ABoatDataSet.Tables(ATable.TableName)
                For Each Trow In TheTable.Rows
                    Dim NewRow As DataRow = ATable.Rows.Add
                    NewRow("BoatID") = BoatID
                    For i = 0 To ATable.Columns.Count - 1
                        For j = 0 To TheTable.Columns.Count - 1
                            If TheTable.Columns(j).ColumnName = ATable.Columns(i).ColumnName Then
                                NewRow(i) = Trow(j)
                                Exit For
                            End If
                        Next
                    Next
                Next
            End If
        Next
    End Sub
    Sub ComputeLevels(ByRef ErrorString As String, Optional Boats As Boolean = True)
        'Dim MyDataSet As DataSet = AnApplication.GetStructureKit.GetData
        Dim CalculateParameter As CalculateParameter = GetCaculateParameter() '计算参数
        Dim SelectSingleDrilling As Boolean = MyDataSet.Tables("LS_Common").Rows(0)("UseSingleDrilling")
        Dim DrillingIDs As New List(Of Integer)
        If SelectSingleDrilling Then
            For Each ARow In MyDataSet.Tables("LS_LegSoilLayer").Rows
                If DrillingIDs.Contains(ARow("DrillingID")) = False Then DrillingIDs.Add(ARow("DrillingID"))
            Next
        Else
            For Each ARow In MyDataSet.Tables("LS_SoilDrilling").Rows
                If DrillingIDs.Contains(ARow("ID")) = False Then DrillingIDs.Add(ARow("ID"))
            Next
        End If
        '清除旧高程
        Dim FilterString As String = ""
        For i = 0 To DrillingIDs.Count - 1
            FilterString &= "DrillingID<>" & DrillingIDs(i) & If(i = DrillingIDs.Count - 1, "", " and ")
        Next
        For Each row In MyDataSet.Tables("LS_CalculationLevels").Select(FilterString, "Level DESC")
            MyDataSet.Tables("LS_CalculationLevels").Rows.Remove(row)
        Next
        For Each DrillingID In DrillingIDs
            Dim Soils As List(Of SoilParameter) = GetSoils(CalculateParameter.UnderWaterPhiSubtractValue, DrillingID, SelectSingleDrilling, ErrorString, Boats) 'Not SelectSingleDrilling'单船计算时，多钻孔共用土层参数表
            If ErrorString <> "" Then
                EsMessageReporter.ReportMessageFunction(ErrorString, EsMessageType.Error)
                Exit Sub
            End If
            Dim ComputeLevels As List(Of Double) = GetComputeLevels(CalculateParameter.NCalculatePoint, Soils, Soils(0).TopLevel, Max(CalculateParameter.DestinationLevel, Soils.Last.TopLevel - 10))
            ''''''''''''''''''''当计算点一致时保留各钻孔各水位处的选择计算模式* *
            Dim OldComputeLevels As New List(Of Double)
            Dim SelectCurrentComMode As Boolean = True
            If ComputeLevels.Count <> MyDataSet.Tables("LS_CalculationLevels").Select("DrillingID=" & DrillingID, "Level DESC").Count Then
                SelectCurrentComMode = False
            Else
                For i = 0 To MyDataSet.Tables("LS_CalculationLevels").Select("DrillingID=" & DrillingID, "Level DESC").Count - 1
                    Dim ARow As DataRow = MyDataSet.Tables("LS_CalculationLevels").Select("DrillingID=" & DrillingID, "Level DESC")(i)
                    OldComputeLevels.Add(ARow("Level"))
                    If ARow("Level") <> Round(ComputeLevels(i), 2) Then
                        SelectCurrentComMode = False
                        Exit For
                    End If
                Next
            End If
            '写入高程
            If SelectCurrentComMode = False Then
                For Each row In MyDataSet.Tables("LS_CalculationLevels").Select("DrillingID=" & DrillingID, "Level DESC")
                    MyDataSet.Tables("LS_CalculationLevels").Rows.Remove(row)
                Next
                For i As Integer = 0 To ComputeLevels.Count - 1
                    Dim NewRow As DataRow
                    NewRow = MyDataSet.Tables("LS_CalculationLevels").Rows.Add
                    NewRow("DrillingID") = DrillingID
                    NewRow("LevelID") = i + 1
                    NewRow("Level") = Round(ComputeLevels(i), 2)
                    NewRow("SelectMode_Qv") = 0
                    NewRow("SelectMode_Qb") = 0
                Next
            End If
        Next
        MyDataSet.AcceptChanges()
    End Sub
    Sub CalculateDepthResult(ByVal IsUserAdd As Boolean, PressLimitValue As Double, Optional ByVal PullLimitValue As Double = 0, Optional ByRef ErrorString As String = "") '默认IsUserAdd=True为用户在界面使用插值法
        '抗压承载力Qv大于单桩抗压力时的持力土的深度即建议插深，同时获得建议插深处的抗拉承载力Qu
        '获得Qv和Qu的插值结果 
        'Dim MyDataSet As DataSet = AnApplication.GetStructureKit.GetData
        If MyDataSet.Tables("LS_CalculationLevels").Rows.Count = 0 Then
            ErrorString = "构件未计算！"
            Exit Sub
        End If
        If IsUserAdd = False Then
            MyDataSet.Tables("LS_DepthResult").Clear()
        Else
            For Each row In MyDataSet.Tables("LS_DepthResult").Select("IsUserAdd=True")
                MyDataSet.Tables("LS_DepthResult").Rows.Remove(row)
            Next
        End If
        Dim ErrorStrings As New List(Of String)
        Dim SelectSingleDrilling As Boolean = MyDataSet.Tables("LS_Common").Rows(0)("UseSingleDrilling")
        Dim LevelIDByDrillingDic As Dictionary(Of Integer, Dictionary(Of Double， Integer)) = GetLevelIDByDrillingDic()
        For Each DrillingID In LevelIDByDrillingDic.Keys
            '获得建议插深  
            GetDepthValue(IsUserAdd, DrillingID, PressLimitValue, True, ErrorString) 'GetInterpolationValue(DrillingID, PressLimitValue, True, ErrorString)'获得Qv的插值结果
            If ErrorStrings.Contains(ErrorString) = False Then ErrorStrings.Add(ErrorString)
            If IsUserAdd Then
                GetDepthValue(IsUserAdd, DrillingID, PullLimitValue, False, ErrorString) 'GetInterpolationValue(DrillingID, PullLimitValue, False, ErrorString)
                If ErrorStrings.Contains(ErrorString) = False Then ErrorStrings.Add(ErrorString)
            End If
        Next
        ErrorString = ""
        For i = 0 To ErrorStrings.Count - 1
            If ErrorStrings(i) <> "" Then
                ErrorString &= ErrorStrings(i) & If(i = ErrorStrings.Count - 1, "", vbCr)
            End If
        Next
    End Sub
    Sub AnalysisOfQv(DrillingID As Integer, ComputeLevelI As Integer, ComputeLevels As List(Of Double), Soils As List(Of SoilParameter), LegParameter As LegParameter, SpudcanParameter As SpudcanParameter, CalculateParameter As CalculateParameter) ', ByRef Qv1 As Double, ByRef Qv1Sand As Double, ByRef Qv1Clay As Double, ByRef Qv2 As Double, ByRef Qv3 As Double, ByRef Qv3Sand As Double, ByRef Qv3Clay As Double, ByRef Qv4 As Double, ByRef QvDescription As String
        Dim Qv1, Qv1Sand, Qv1Clay, Qv2, Qv3, Qv3Sand, Qv3Clay, Qv4 As Double
        Dim QvDescription As String = ""
        Qv1 = 10 ^ 10
        Qv1Sand = 10 ^ 10
        Qv1Clay = 10 ^ 10
        Qv2 = 10 ^ 10
        Qv3 = 10 ^ 10
        Qv3Sand = 10 ^ 10
        Qv3Clay = 10 ^ 10
        Qv4 = 10 ^ 10

        Dim ComputeLevel As Double = ComputeLevels(ComputeLevelI)
        Dim Soil As SoilParameter = GetSoil(ComputeLevel, Soils) '持力土层
        Dim CalSpudcanB As Double = SpudcanParameter.GetSpudcanB()
        If Soil.SoilType <> SoilType.Clay Then CalSpudcanB = If(CalculateParameter.IsEquivalentToCircleSpudcan, 2 * (SpudcanParameter.Area / PI) ^ 0.5, SpudcanParameter.GetSpudcanB()) '20230804
        If GetIsDownSoilTypeExtra(ComputeLevel, CalSpudcanB, Soils) Then '判断计算高程以下B/2内是否有3种或3中以上的土
            EsMessageReporter.ReportMessageFunction("场地内" & ComputeLevels(ComputeLevelI) & "m处有软、硬土层交错分布风险，请谨慎使用计算结果", EsMessageType.Warning)
        End If

        Dim UseNormal As Boolean = False
        Dim NextLevel As Double = If(ComputeLevelI = ComputeLevels.Count - 1, ComputeLevel, ComputeLevels(ComputeLevelI + 1))
        Dim NextSoilLevel As Double = Min(NextLevel, Soil.BottomLevel)
        Dim MergeSandSoil As Boolean = False
        NextSoilLevel = Min(NextSoilLevel, GetDownSandLayersNextLevel(ComputeLevel, Soils, MergeSandSoil))
        If ComputeLevel - NextSoilLevel > CalSpudcanB Then
            UseNormal = True
            EsMessageReporter.ReportMessageFunction("抗压承载力计算：持力层土厚度" & Round(ComputeLevel - NextSoilLevel, 2) & "大于B(" & Round(CalSpudcanB, 2) & ")，按常规破坏模式计算", EsMessageType.Normal)
            '当多层砂土需要合并土层，且计算高程不为最底计算高程，合并土层高度=Min(等效宽度，砂土层总高)20250703
            If MergeSandSoil And ComputeLevelI <> ComputeLevels.Count - 1 Then
                NextSoilLevel = ComputeLevel - CalSpudcanB
            End If
        Else
            MyDataSet.Tables("LS_CalculationLevels").Compute("Min(Level)", "DrillingID=" & DrillingID) '多层砂土需要合并土层，合并土层高度=Min(等效宽度，砂土层总高)20250703
            EsMessageReporter.ReportMessageFunction("抗压承载力计算：持力层土厚度" & Round(ComputeLevel - NextSoilLevel, 2) & "小于等于B(" & Round(CalSpudcanB, 2) & ")，按挤出破坏模式和穿刺破坏模式计算", EsMessageType.Normal)
        End If
        If Soil.SoilType <> SoilType.Clay Then
            If UseNormal Then
                Qv1Sand = GetQV_Sand(CalculateParameter.IsEquivalentToCircleSpudcan, CalSpudcanB, ComputeLevel, NextSoilLevel, LegParameter, SpudcanParameter, Soil, Soils, CalculateParameter.IsBackFlow, CalculateParameter.Hc, QvDescription, "Qv1_Sand")
            Else
                Qv3Sand = GetQV_Punch_Sand(CalculateParameter.IsEquivalentToCircleSpudcan, CalSpudcanB, ComputeLevel, NextSoilLevel, LegParameter, SpudcanParameter, Soil, Soils, CalculateParameter.IsBackFlow, CalculateParameter.Hc, QvDescription, "Qv3_Sand")
            End If
        End If
        If Soil.SoilType <> SoilType.Sand Then
            If UseNormal Then
                Qv1Clay = GetQV_Clay(CalSpudcanB, ComputeLevel, LegParameter, SpudcanParameter, Soil, Soils, CalculateParameter.IsBackFlow, CalculateParameter.Hc, QvDescription, "Qv1_Clay")
            Else
                Qv3Clay = GetQV_Punch_Clay(CalSpudcanB, ComputeLevel, NextSoilLevel, LegParameter, SpudcanParameter, Soil, Soils, CalculateParameter.IsBackFlow, CalculateParameter.Hc, QvDescription, "Qv3_Clay")
            End If
        End If
        Select Case Soil.SoilType
            Case SoilType.Both
                If UseNormal Then
                    Qv1 = Min(Qv1Sand, Qv1Clay)
                    QvDescription &= "Qv3未计算;Qv3_Clay未计算;Qv3_Sand未计算;"
                Else
                    Qv3 = Min(Qv3Sand, Qv3Clay)
                    QvDescription &= "Qv1未计算;Qv1_Clay未计算;Qv1_Sand未计算;"
                End If
            Case SoilType.Sand
                If UseNormal Then
                    Qv1 = Qv1Sand
                    QvDescription &= "Qv3未计算;Qv3_Sand未计算;"
                Else
                    Qv3 = Qv3Sand
                    QvDescription &= "Qv1未计算;Qv1_Sand未计算;"
                End If
                QvDescription &= "Qv1_Clay未计算;Qv3_Clay未计算;"
            Case SoilType.Clay
                If UseNormal Then
                    Qv1 = Qv1Clay
                    QvDescription &= "Qv3未计算;Qv3_Clay未计算;"
                Else
                    Qv3 = Qv3Clay
                    QvDescription &= "Qv1未计算;Qv1_Clay未计算;"
                End If
                QvDescription &= "Qv1_Sand未计算;Qv3_Sand未计算;"
        End Select
        'Qv1 = If(Soil.SoilType = SoilType.Both, Min(Qv1Sand, Qv1Clay), If(Soil.SoilType = SoilType.Clay, Qv1Clay, Qv1Sand))
        'Qv3 = If(Soil.SoilType = SoilType.Both, Min(Qv3Sand, Qv3Clay), If(Soil.SoilType = SoilType.Clay, Qv3Clay, Qv3Sand))
        If UseNormal Then
            QvDescription &= "Qv2未计算;"
        Else
            Qv2 = GetQV_Squeeze(CalculateParameter.IsEquivalentToCircleSpudcan, CalSpudcanB, ComputeLevel, NextLevel, LegParameter, SpudcanParameter, Soil, Soils, CalculateParameter.IsBackFlow, CalculateParameter.Hc, QvDescription, "Qv2")
        End If
        Qv4 = GetQV_MultiLayer(CalSpudcanB, ComputeLevel, LegParameter, SpudcanParameter, Soil, Soils, CalculateParameter.IsBackFlow, CalculateParameter.Hc, QvDescription, "Qv4")

        AnalysisOfQv_WriteResult(DrillingID, ComputeLevelI, ComputeLevel, Soil, SpudcanParameter.Area, Qv1, Qv1Sand, Qv1Clay, Qv2, Qv3, Qv3Sand, Qv3Clay, Qv4, QvDescription)
    End Sub
    Sub AnalysisOfQv_WriteResult(DrillingID As Integer, ComputeLevelI As Integer, ComputeLevel As Double, Soil As SoilParameter， SpudcanParameterA As Double, ByRef Qv1 As Double, ByRef Qv1Sand As Double, ByRef Qv1Clay As Double, ByRef Qv2 As Double, ByRef Qv3 As Double, ByRef Qv3Sand As Double, ByRef Qv3Clay As Double, ByRef Qv4 As Double, ByRef QvDescription As String)
        Dim NewRow As DataRow
        QvDescription = QvDescription.Remove(QvDescription.Length - 1, 1)
        NewRow = MyDataSet.Tables("LS_PressResistanceResult").Rows.Add
        NewRow("DrillingID") = DrillingID
        NewRow("ID") = ComputeLevelI + 1
        NewRow("Level") = Round(ComputeLevel, 2)
        NewRow("Qv1") = Round(Qv1, 2)
        NewRow("Qv1_Sand") = Round(Qv1Sand, 2)
        NewRow("Qv1_Clay") = Round(Qv1Clay, 2)
        NewRow("Qv2") = Round(Qv2, 2)
        NewRow("Qv3") = Round(Qv3, 2)
        NewRow("Qv3_Sand") = Round(Qv3Sand, 2)
        NewRow("Qv3_Clay") = Round(Qv3Clay, 2)
        NewRow("Qv4") = Round(Qv4, 2)
        NewRow("Description") = QvDescription
        NewRow("SoilID") = Soil.SoilID
        NewRow("IsSand") = Soil.SoilType '***
        'If SelectCurrentComMode = False Then
        '    NewRow("Qvp") = Round(Min(Qv1, Min(Qv2, Qv3)) / SpudcanParameter.Area, 2)
        '    NewRow("Qv") = Round(Min(Qv1, Min(Qv2, Qv3)), 2)
        '    NewRow("SelectMode") = 0
        'Else
        Dim QvSelectMode As Integer = Integer.Parse(MyDataSet.Tables("LS_CalculationLevels").Select("Level=" & NewRow("Level").ToString & " and DrillingID=" & DrillingID)(0)("SelectMode_Qv").ToString)
        Select Case QvSelectMode
            Case 0
                NewRow("Qvp") = Round(If(Min(Qv1, Min(Qv2, Qv3)) = 10 ^ 10, 10 ^ 10, Min(Qv1, Min(Qv2, Qv3)) / SpudcanParameterA), 2)
                NewRow("Qv") = Round(Min(Qv1, Min(Qv2, Qv3)), 2)
                NewRow("SelectMode") = 0
            Case 1
                NewRow("Qvp") = If(Qv1 = 10 ^ 10, 10 ^ 10, Round(Qv1 / SpudcanParameterA, 2))
                NewRow("Qv") = Round(Qv1, 2)
                NewRow("SelectMode") = 1
            Case 2
                NewRow("Qvp") = If(Qv4 = 10 ^ 10, 10 ^ 10, Round(Qv4 / SpudcanParameterA, 2))
                NewRow("Qv") = Round(Qv4, 2)
                NewRow("SelectMode") = 2
            Case 3
                NewRow("Qvp") = If(Qv2 = 10 ^ 10, 10 ^ 10, Round(Qv2 / SpudcanParameterA, 2))
                NewRow("Qv") = Round(Qv2, 2)
                NewRow("SelectMode") = 3
            Case 4
                NewRow("Qvp") = If(Qv3 = 10 ^ 10, 10 ^ 10, Round(Qv3 / SpudcanParameterA, 2))
                NewRow("Qv") = Round(Qv3, 2)
                NewRow("SelectMode") = 4
        End Select
        'End If
        Dim ResultTitle As String() = {"QvP", "Qv", "Qv1", "Qv1_Sand", "Qv1_Clay", "Qv2", "Qv3", "Qv3_Sand", "Qv3_Clay", "Qv4"}
        For Each Title In ResultTitle
            If NewRow(Title) = (10 ^ 10).ToString Then NewRow(Title) = "-"
        Next
        '对上层土挤出破坏的结果进行比对* *20240429
        '挤出破坏模式计算的竖向承载力下限值为软土的常规破坏模式承载力计算结果，上限值为下层硬土的承载力计算结果
        Dim RC As Integer = MyDataSet.Tables("LS_PressResistanceResult").Rows.Count
        If ComputeLevelI >= 1 AndAlso MyDataSet.Tables("LS_PressResistanceResult").Rows(RC - 2)("Qv2") <> "-" Then
            '获得上限值
            Dim Ri As DataRow = MyDataSet.Tables("LS_PressResistanceResult").Rows(RC - 2)
            'Dim NewRow As DataRow = MyDataSet.Tables("LS_PressResistanceResult").Rows(RC - 1)
            If Double.Parse(Ri("Qv2")) > Double.Parse(NewRow("Qv")) Then
                If Ri("Qv") = Ri("Qv2") Then
                    Ri("Qv") = NewRow("Qv")
                    Ri("Qvp") = NewRow("Qvp")
                End If
                '获得对应备注 
                Dim TempQv2Description As String = ""
                Dim Qvs As String() = {"Qv1_Sand", "Qv1_Clay", "Qv2", "Qv3_Sand", "Qv3_Clay", "Qv4", "Qv", "QvP"}
                For Qi = 0 To Qvs.Count - 3
                    If NewRow("Qv") = NewRow(Qvs(Qi)) Then
                        Ri("Description") = GetQvDescription(Ri("Description"), NewRow("Description"), Qvs(Qi), "Qv2", "Qv=Min(Qv，持力+1层土Qv)")
                        Exit For
                    End If
                Next
                Dim NormalString As String = Ri("Level").ToString & "m挤出破坏模式：挤出破坏结果Qv=Min(挤出Qv(" & Ri("Qv2").ToString & ")，持力+1层土Qv(" & NewRow("Qv").ToString & ")"
                Ri("Qv2") = NewRow("Qv")
                EsMessageReporter.ReportMessageFunction(NormalString, EsMessageType.Normal)
            End If
        End If
    End Sub
    Sub AssessmentOfPunctureRisk(DrillingID As Integer, PressLimitValue As Double, SpudcanParameter As SpudcanParameter)
        Dim SpudcanB As Double = SpudcanParameter.GetSpudcanB()
        Dim betaBDepth, P1, P2, P3, P2Level, P3Level As Double
        P1 = PressLimitValue
        P2 = -10 ^ 10
        P2Level = 10 ^ 10
        P3 = 10 ^ 10
        P3Level = 10 ^ 10
        betaBDepth = 1 * SpudcanB
        Dim SelectString As String = "DrillingID=" & DrillingID & " and Qv<>'-'"
        Dim TheLevel As Double
        For Each PRRRow In MyDataSet.Tables("LS_PressResistanceResult").Select(SelectString, "Level DESC")
            If Val(PRRRow("Qv")) >= PressLimitValue Then
                TheLevel = Val(PRRRow("Level"))
                SelectString = "DrillingID=" & DrillingID & " and Qv<>'-' and Level>=" & TheLevel - betaBDepth & " and Level<" & TheLevel
                For Each Row In MyDataSet.Tables("LS_PressResistanceResult").Select(SelectString, "Level DESC")
                    P3 = Min(P3, Val(Row("Qv")))
                    P3Level = If(P3 = Val(Row("Qv")), Val(Row("Level")), P3Level)
                Next
                P2 = -10 ^ 10
                P2Level = 10 ^ 10
                SelectString = "DrillingID=" & DrillingID & " and Qv<>'-' and Level>=" & TheLevel - betaBDepth & " and Level<=" & TheLevel
                For Each Row In MyDataSet.Tables("LS_PressResistanceResult").Select(SelectString, "Level DESC")
                    If P2 <= Val(Row("Qv")) Then
                        P2 = Val(Row("Qv"))
                        P2Level = Val(Row("Level"))
                    Else
                        Exit For
                    End If
                Next
                Exit For
            End If
        Next
        If P3Level = P2Level Then P3 = 10 ^ 10 '
        Dim NewRow As DataRow = MyDataSet.Tables("LS_PunctureRiskAssessmentResult").Rows.Add
        NewRow("DrillingID") = DrillingID
        NewRow("P1") = Round(P1 / SpudcanParameter.Area, 2) '(kPa)
        NewRow("P2") = If(P2 = -10 ^ 10, "-", Round(P2 / SpudcanParameter.Area, 2).ToString)
        NewRow("P3") = If(P3 = 10 ^ 10, "-", Round(P3 / SpudcanParameter.Area, 2).ToString)
        NewRow("Fs1") = If(P2 = -10 ^ 10, "-", Round(P2 / P1, 2).ToString)
        NewRow("Fs2") = If(P3 = 10 ^ 10, "-", Round(P3 / P1, 2).ToString)
        NewRow("IsPunctureRiskOK") = P2 <> -10 ^ 10 AndAlso P2 / P1 >= 1.5 OrElse P3 = 10 ^ 10 OrElse P3 / P1 >= 1.2
    End Sub
    Sub AnalysisOfQb(DrillingID As Integer, ComputeLevelI As Integer, ComputeLevels As List(Of Double), Soils As List(Of SoilParameter), LegParameter As LegParameter, SpudcanParameter As SpudcanParameter, CalculateParameter As CalculateParameter)
        Dim Qb(2), QbSand(2), QbClay(2) As Double, DeepType, DeepTypeSand, DeepTypeClay As Integer
        Dim QbDescription As String = ""
        Qb(0) = 10 ^ 10
        Qb(1) = 10 ^ 10
        Qb(2) = 10 ^ 10
        QbSand(0) = 10 ^ 10
        QbSand(1) = 10 ^ 10
        QbSand(2) = 10 ^ 10
        QbClay(0) = 10 ^ 10
        QbClay(1) = 10 ^ 10
        QbClay(2) = 10 ^ 10

        Dim ComputeLevel As Double = ComputeLevels(ComputeLevelI)
        Dim Soil As SoilParameter = GetSoil(ComputeLevel, Soils) '持力土层
        Dim fb As Double = CalculateParameter.fb
        Dim B As Double = SpudcanParameter.GetSpudcanB()
        Dim D As Double = Soils(0).TopLevel - ComputeLevel
        Dim AverageSoil As SoilParameter = GetAverageSoilValue(Soils, ComputeLevel, Soils(0).TopLevel)
        Dim H As Double = GetH(AverageSoil.Phi, SpudcanParameter.B) '判别深度H，插深D处往上高度，不考虑fb20230620
        Dim IsSameSoilType As Boolean = GetIsSameUpSoilType(ComputeLevel, Soils)
        If IsSameSoilType Then
            If Soil.SoilType <> SoilType.Clay Then 'Soil.SoilType = SoilType.Both Or Soil.SoilType = SoilType.Sand
                EsMessageReporter.ReportMessageFunction("抗拔承载力计算：桩靴穿过均质土，持力层土类型为砂土，按砂土拔桩力计算", EsMessageType.Normal)
                DeepTypeSand = If(H < D, 3, 1)
                EsMessageReporter.ReportMessageFunction("砂土拔桩力：判别深度H(" & H & "m)" & If(H < D, "＜", "≥") & "插深D(" & D & "m)，按" & If(H < D, "深埋", "浅埋") & "计算", EsMessageType.Normal)
                QbSand = GetQb_Sand(DeepTypeSand, CalculateParameter, ComputeLevel, LegParameter, SpudcanParameter, Soil, Soils, fb, H, QbDescription, "Qu_Sand")
            End If
            If Soil.SoilType <> SoilType.Sand Then
                EsMessageReporter.ReportMessageFunction("抗拔承载力计算：桩靴穿过均质土，持力层土类型为粘土，按粘土拔桩力计算", EsMessageType.Normal)
                DeepTypeClay = If(B < D, 3, 1)
                EsMessageReporter.ReportMessageFunction("粘土拔桩力：桩靴宽度B(" & B & "m)" & If(B < D, "＜", "≥") & "插深D(" & D & "m)，按" & If(B < D, "深埋", "浅埋") & "计算", EsMessageType.Normal)
                QbClay = GetQb_Clay(DeepTypeClay, CalculateParameter, ComputeLevel, LegParameter, SpudcanParameter, Soil, Soils, fb, QbDescription, "Qu_Clay")
            End If
        Else
            EsMessageReporter.ReportMessageFunction("抗拔承载力计算：桩靴穿过多层砂粘土互层，分别按砂土拔桩力和粘土拔桩力计算", EsMessageType.Normal)
            DeepTypeSand = If(H < D, 3, 1)
            EsMessageReporter.ReportMessageFunction("砂土拔桩力：判别深度H(" & H & "m)" & If(H < D, "＜", "≥") & "插深D(" & D & "m)，按" & If(H < D, "深埋", "浅埋") & "计算", EsMessageType.Normal)
            QbSand = GetQb_Sand(DeepTypeSand, CalculateParameter, ComputeLevel, LegParameter, SpudcanParameter, Soil, Soils, fb, H, QbDescription, "Qu_Sand")
            DeepTypeClay = If(B < D, 3, 1)
            EsMessageReporter.ReportMessageFunction("粘土拔桩力：桩靴宽度B(" & B & "m)" & If(B < D, "＜", "≥") & "插深D(" & D & "m)，按" & If(B < D, "深埋", "浅埋") & "计算", EsMessageType.Normal)
            QbClay = GetQb_Clay(DeepTypeClay, CalculateParameter, ComputeLevel, LegParameter, SpudcanParameter, Soil, Soils, fb, QbDescription, "Qu_Clay")
        End If
        For fbi = 0 To 2
            If Soil.SoilType = SoilType.Both Or IsSameSoilType = False Then
                Qb(fbi) = Max(QbSand(fbi), QbClay(fbi))
                DeepType = If(Qb(fbi) = QbClay(fbi), DeepTypeClay, DeepTypeSand)
            Else
                If Soil.SoilType = SoilType.Clay Then
                    Qb(fbi) = QbClay(fbi)
                    DeepType = DeepTypeClay
                    DeepTypeSand = 0
                    QbDescription &= If(fbi = 2, "Qu_Sand", "Qu_S" & fbi) & "未计算;"
                Else
                    Qb(fbi) = QbSand(fbi)
                    DeepType = DeepTypeSand
                    DeepTypeClay = 0
                    QbDescription &= If(fbi = 2, "Qu_Clay", "Qu_C" & fbi) & "未计算;"
                End If
            End If
        Next

        AnalysisOfQb_WriteResult(DrillingID, ComputeLevelI, ComputeLevel, Soil, Qb, QbSand, QbClay, DeepType, DeepTypeSand, DeepTypeClay, QbDescription)
    End Sub
    Sub AnalysisOfQb_WriteResult(DrillingID As Integer, ComputeLevelI As Integer, ComputeLevel As Double, Soil As SoilParameter, ByRef Qb As Double(), ByRef QbSand As Double(), ByRef QbClay As Double(), ByRef DeepType As Integer, ByRef DeepTypeSand As Integer, ByRef DeepTypeClay As Integer, ByRef QbDescription As String)
        Dim NewRow As DataRow
        QbDescription = QbDescription.Remove(QbDescription.Length - 1, 1)
        NewRow = MyDataSet.Tables("LS_PullResistanceResult").Rows.Add
        NewRow("DrillingID") = DrillingID
        NewRow("ID") = ComputeLevelI + 1
        NewRow("SoilID") = Soil.SoilID
        NewRow("Level") = Round(ComputeLevel, 2)
        NewRow("DeepType_Sand") = DeepTypeSand
        NewRow("Qu_Sand") = Round(QbSand(2), 2)
        NewRow("DeepType_Clay") = DeepTypeClay
        NewRow("Qu_Clay") = Round(QbClay(2), 2)
        NewRow("Description") = QbDescription
        'If SelectCurrentComMode = False Then
        '    NewRow("Qu") = Round(Qb, 2)
        '    NewRow("QuP") = Round(Qb / 9.8, 2)
        '    NewRow("DeepType") = DeepType
        '    NewRow("SelectMode") = 0
        'Else
        Dim QuSelectMode As Integer = Integer.Parse(MyDataSet.Tables("LS_CalculationLevels").Select("Level=" & NewRow("Level") & " and DrillingID=" & DrillingID)(0)("SelectMode_Qb").ToString)
        Select Case QuSelectMode
            Case 0
                NewRow("Qu") = Round(Qb(2), 2)
                NewRow("QuP") = If(Qb(2) = 10 ^ 10, 10 ^ 10, Round(Qb(2) / 9.8, 2))
                NewRow("DeepType") = DeepType
                NewRow("SelectMode") = 0
                NewRow("Qu0") = Round(Qb(0), 2)
                NewRow("Qu1") = Round(Qb(1), 2)
            Case 1
                NewRow("Qu") = Round(QbSand(2), 2)
                NewRow("QuP") = If(QbSand(2) = 10 ^ 10, 10 ^ 10, Round(QbSand(2) / 9.8, 2))
                NewRow("DeepType") = DeepTypeSand
                NewRow("SelectMode") = 1
                NewRow("Qu0") = Round(QbSand(0), 2)
                NewRow("Qu1") = Round(QbSand(1), 2)
            Case 2
                NewRow("Qu") = Round(QbClay(2), 2)
                NewRow("QuP") = If(QbClay(2) = 10 ^ 10, 10 ^ 10, Round(QbClay(2) / 9.8, 2))
                NewRow("DeepType") = DeepTypeClay
                NewRow("SelectMode") = 2
                NewRow("Qu0") = Round(QbClay(0), 2)
                NewRow("Qu1") = Round(QbClay(1), 2)
        End Select
        'End If
        Dim ResultTitle As String() = {"QuP", "Qu", "Qu_Sand", "Qu_Clay", "Qu0", "Qu1"}
        For Each Title In ResultTitle
            If NewRow(Title) = (10 ^ 10).ToString Then NewRow(Title) = "-"
        Next
    End Sub
    Sub GetDepthValue(ByVal IsUserAdd As Boolean, DrillingID As Integer, LimitValue As Double, IsPressValue As Boolean, ByRef ErrorString As String)
        '获得Qv和Qu的插值结果
        Dim NewRow As DataRow
        'Dim MyDataSet As DataSet = AnApplication.GetStructureKit.GetData
        Dim TopLevel As Double = MyDataSet.Tables("LS_CalculationLevels").Compute("Max(Level)", "DrillingID=" & DrillingID)
        Dim ResultTab, AnotherResultTab As DataTable ' = If(IsPressValue, MyDataSet.Tables("LS_PressResistanceResult"), MyDataSet.Tables("LS_PullResistanceResult"))
        Dim SelectParam, AnotherSelectParam As String  'If(IsPressValue, "Qv", "Qu")
        If IsPressValue Then
            ResultTab = MyDataSet.Tables("LS_PressResistanceResult")
            SelectParam = "Qv"
            AnotherResultTab = MyDataSet.Tables("LS_PullResistanceResult")
            AnotherSelectParam = "Qu"
        Else
            ResultTab = MyDataSet.Tables("LS_PullResistanceResult")
            SelectParam = "Qu"
            AnotherResultTab = MyDataSet.Tables("LS_PressResistanceResult")
            AnotherSelectParam = "Qv"
        End If
        Dim Level As Double
        ErrorString = "不在范围内！"
        If ResultTab.Select("DrillingID=" & DrillingID & " and " & SelectParam & "<>'-'", "Level DESC").Count > 0 Then
            For Each TheRow In ResultTab.Select("DrillingID=" & DrillingID & " and " & SelectParam & "<>'-'", "Level DESC")
                If Val(TheRow(SelectParam)) > LimitValue Then
                    Level = Val(TheRow("Level"))
                    NewRow = MyDataSet.Tables("LS_DepthResult").Rows.Add
                    NewRow("LimitForce") = Double.Parse(LimitValue.ToString("N2"))
                    NewRow("IsUserAdd") = IsUserAdd 'True
                    NewRow("DrillingID") = DrillingID
                    NewRow(SelectParam) = TheRow(SelectParam)
                    If SelectParam = "Qu" Then
                        NewRow("Qu0") = TheRow("Qu0")
                        NewRow("Qu1") = TheRow("Qu1")
                    End If
                    NewRow("SuggestedDepth") = Double.Parse(Level.ToString("N2")) 'Double.Parse((TopLevel - Level).ToString("N2"))'自升式平台桩腿插拔计算软件V1.0软件修改要求 2023-4-26.docx-修改6
                    NewRow("SupportSoilID") = TheRow("SoilID")
                    Dim SoilRow As DataRow = MyDataSet.Tables("LS_Soil").Select("ID=" & TheRow("SoilID"))(0)
                    NewRow("SupportSoilStrength") = If(SoilRow("Type") = 1, SoilRow("UnderWaterPhi"), SoilRow("Su0"))
                    TheRow = AnotherResultTab.Select("DrillingID=" & DrillingID & " and Level=" & Level, "Level DESC")(0)
                    NewRow(AnotherSelectParam) = If(TheRow(AnotherSelectParam) = "-", "-", TheRow(AnotherSelectParam))
                    If AnotherSelectParam = "Qu" Then
                        NewRow("Qu0") = TheRow("Qu0")
                        NewRow("Qu1") = TheRow("Qu1")
                    End If
                    'NewRow("SelectMode_Qv") = 0 '插值计算无用
                    ErrorString = ""
                    Exit For
                End If
            Next
        End If
    End Sub
    Sub GetInterpolationValue(DrillingID As Integer, LimitValue As Double, IsPressValue As Boolean, ByRef ErrorString As String)
        '获得Qv和Qu的插值结果
        Dim NewRow As DataRow
        'Dim MyDataSet As DataSet = AnApplication.GetStructureKit.GetData
        Dim TopLevel As Double = MyDataSet.Tables("LS_CalculationLevels").Compute("Max(Level)", "DrillingID=" & DrillingID)
        Dim LevelIDByDrillingDic As Dictionary(Of Integer, Dictionary(Of Double， Integer)) = GetLevelIDByDrillingDic()
        Dim ResultTab, AnotherResultTab As DataTable ' = If(IsPressValue, MyDataSet.Tables("LS_PressResistanceResult"), MyDataSet.Tables("LS_PullResistanceResult"))
        Dim SelectParam, AnotherSelectParam As String  'If(IsPressValue, "Qv", "Qu")
        If IsPressValue Then
            ResultTab = MyDataSet.Tables("LS_PressResistanceResult")
            SelectParam = "Qv"
            AnotherResultTab = MyDataSet.Tables("LS_PullResistanceResult")
            AnotherSelectParam = "Qu"
        Else
            ResultTab = MyDataSet.Tables("LS_PullResistanceResult")
            SelectParam = "Qu"
            AnotherResultTab = MyDataSet.Tables("LS_PressResistanceResult")
            AnotherSelectParam = "Qv"
        End If
        Dim UpLevel, UpValue, DownLevel, DownValue As Double
        Dim UpResultDic As New Dictionary(Of Double, Double)
        Dim DownResultDic As New Dictionary(Of Double, Double)
        For Each ARow In ResultTab.Select("DrillingID=" & DrillingID & " and " & SelectParam & "<>'-'", "Level DESC")
            If Double.Parse(ARow(SelectParam)) <= LimitValue And UpResultDic.ContainsKey(ARow("Level")) = False Then
                If DownResultDic.Count = 0 OrElse DownResultDic.First.Key < ARow("Level") Then
                    UpResultDic.Add(ARow("Level"), Double.Parse(ARow(SelectParam)))
                End If
            End If
            If Double.Parse(ARow(SelectParam)) >= LimitValue And DownResultDic.ContainsKey(ARow("Level")) = False Then
                DownResultDic.Add(ARow("Level"), Double.Parse(ARow(SelectParam)))
            End If
        Next
        If UpResultDic.Keys.Count <> 0 And DownResultDic.Keys.Count <> 0 Then
            'If UpValue <= LimitValue And DownValue >= LimitValue Then
            DownLevel = DownResultDic.First.Key
            DownValue = DownResultDic.First.Value
            UpLevel = UpResultDic.Last.Key
            UpValue = UpResultDic.Last.Value
            Dim Level As Double = DownLevel + (DownValue - LimitValue) * (UpLevel - DownLevel) / (DownValue - UpValue)
            NewRow = MyDataSet.Tables("LS_DepthResult").Rows.Add
            NewRow("LimitForce") = Double.Parse(LimitValue.ToString("N2"))
            NewRow("IsUserAdd") = True 'IsUserAdd
            NewRow("DrillingID") = DrillingID
            NewRow(SelectParam) = LimitValue.ToString("N2")
            NewRow("SuggestedDepth") = Double.Parse((TopLevel - Level).ToString("N2"))
            Dim SoilID As Integer
            Dim TempResult As Double
            If LevelIDByDrillingDic(DrillingID).ContainsKey(Level) Then
                If Double.TryParse(AnotherResultTab.Select("DrillingID=" & DrillingID & " and Level=" & Level, "Level DESC")(0)(AnotherSelectParam), TempResult) = False Then
                    TempResult = 10 ^ 10
                End If
                SoilID = LevelIDByDrillingDic(DrillingID)(Level)
            Else
                Dim UpRow1 As DataRow = AnotherResultTab.Select("DrillingID=" & DrillingID & " and Level>=" & Level & " and " & AnotherSelectParam & "<>'-'", "Level DESC").Last
                Dim DownRow1 As DataRow = AnotherResultTab.Select("DrillingID=" & DrillingID & " and Level<=" & Level & " and " & AnotherSelectParam & "<>'-'", "Level DESC").First
                TempResult = Double.Parse(DownRow1(AnotherSelectParam)) + (DownRow1("Level") - Level) * (Double.Parse(UpRow1(AnotherSelectParam)) - Double.Parse(DownRow1(AnotherSelectParam))) / (DownRow1("Level") - UpRow1("Level"))
                For i = 0 To LevelIDByDrillingDic(DrillingID).Keys.Count - 1
                    Dim Level1 = LevelIDByDrillingDic(DrillingID).Keys(i)
                    Dim Level2 = LevelIDByDrillingDic(DrillingID).Keys(i + 1)
                    If Level1 > Level And Level2 < Level Then
                        SoilID = LevelIDByDrillingDic(DrillingID)(Level2)
                        Exit For
                    End If
                Next
            End If
            NewRow(AnotherSelectParam) = If(TempResult = 10 ^ 10, "-", TempResult.ToString("N2"))
            NewRow("SupportSoilID") = SoilID
            Dim SoilRow As DataRow = MyDataSet.Tables("LS_Soil").Select("ID=" & SoilID)(0)
            NewRow("SupportSoilStrength") = If(SoilRow("Type") = 1, SoilRow("UnderWaterPhi"), SoilRow("Su0"))
            'NewRow("SelectMode_Qv") = 0 '插值计算无用
            ErrorString = ""
        Else
            ErrorString = "不在范围内！"
        End If
    End Sub
    Function GetLevelIDByDrillingDic()
        'Dim MyDataSet As DataSet = AnApplication.GetStructureKit.GetData
        Dim LevelIDByDrillingDic As New Dictionary(Of Integer, Dictionary(Of Double， Integer))
        Dim SelectSingleDrilling As Boolean = MyDataSet.Tables("LS_Common").Rows(0)("UseSingleDrilling")
        If SelectSingleDrilling Then
            For Each ARow In MyDataSet.Tables("LS_LegSoilLayer").Select("", "TopLevel DESC")
                If LevelIDByDrillingDic.ContainsKey(ARow("DrillingID")) = False Then LevelIDByDrillingDic.Add(ARow("DrillingID"), New Dictionary(Of Double， Integer))
                LevelIDByDrillingDic(ARow("DrillingID")).Add(ARow("TopLevel"), ARow("SoilID"))
            Next
        Else
            For Each ARow In MyDataSet.Tables("LS_SoilDrilling").Rows
                If LevelIDByDrillingDic.ContainsKey(ARow("ID")) = False Then LevelIDByDrillingDic.Add(ARow("ID"), New Dictionary(Of Double， Integer))
                Dim SoilLayers As String = ARow("SoilLayers")
                For Each ALayer In SoilLayers.Split(";")
                    LevelIDByDrillingDic(ARow("ID")).Add(Val(ALayer.Split(",")(1)), MyDataSet.Tables("LS_Soil").Select("Name='" & ALayer.Split(",")(0) & "'")(0)("ID"))
                Next
            Next
        End If
        Return LevelIDByDrillingDic
    End Function
    Sub CaculateByEquation(Optional ByVal Boats As Boolean = False)
        On Error Resume Next
        WarningMessageList = New List(Of String)
        ErrorMessageList = New List(Of String)
        'Dim MyDataSet As DataSet = AnApplication.GetStructureKit.GetData
        'Dim WaterWeight As Double = 10 '饱和重度更改为浮重度
        Dim LegParameter As LegParameter = GetLegParameter() '腿参数
        Dim SpudcanParameter As SpudcanParameter = GetSpudcanParameter() '桩靴参数
        Dim CalculateParameter As CalculateParameter = GetCaculateParameter() '计算参数
        Dim SelectSingleDrilling As Boolean = MyDataSet.Tables("LS_Common").Rows(0)("UseSingleDrilling")
        MyDataSet.Tables("LS_Holl").Clear()
        MyDataSet.Tables("LS_PressResistanceResult").Clear()
        MyDataSet.Tables("LS_PullResistanceResult").Clear()
        MyDataSet.Tables("LS_PunctureRiskAssessmentResult").Clear()
        Dim PressLimitValue As Double = Round(CalculateParameter.PressForce * 9.8, 6)

        Dim DrillingIDs As New List(Of Integer)
        If SelectSingleDrilling Then
            For Each ARow In MyDataSet.Tables("LS_LegSoilLayer").Rows
                If DrillingIDs.Contains(ARow("DrillingID")) = False Then DrillingIDs.Add(ARow("DrillingID"))
            Next
        Else
            For Each ARow In MyDataSet.Tables("LS_SoilDrilling").Rows
                If DrillingIDs.Contains(ARow("ID")) = False Then DrillingIDs.Add(ARow("ID"))
            Next
        End If
        For Each DrillingID In DrillingIDs
            EsMessageReporter.ReportMessageFunction("计算准备,计算编号" & DrillingID & "钻孔>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>", EsMessageType.Normal)
            Dim ErrorString As String = ""
            Dim Soils As List(Of SoilParameter) = GetSoils(CalculateParameter.UnderWaterPhiSubtractValue, DrillingID, SelectSingleDrilling, ErrorString, Boats) 'WaterWeight,'GetSoils(WaterWeight) '土层参数
            If ErrorString <> "" Then
                EsMessageReporter.ReportMessageFunction(ErrorString, EsMessageType.Error)
                Exit Sub
            End If
            Dim ComputeLevels As New List(Of Double)
            For Each LRow In MyDataSet.Tables("LS_CalculationLevels").Select("DrillingID=" & DrillingID, "Level DESC")
                ComputeLevels.Add(LRow("Level"))
            Next
            '计算极限洞深 
            GetHc(DrillingID, SpudcanParameter, CalculateParameter, Soils)
            '计算承载力
            For i As Integer = 0 To ComputeLevels.Count - 1
                EsMessageReporter.ReportProgressFunction(100 * (i + 1) / ComputeLevels.Count)
                EsMessageReporter.ReportMessageFunction("计算高程=" & ComputeLevels(i), EsMessageType.Normal)
                '计算抗压
                AnalysisOfQv(DrillingID, i, ComputeLevels, Soils, LegParameter, SpudcanParameter, CalculateParameter) ', Qv1, Qv1Sand, Qv1Clay, Qv2, Qv3, Qv3Sand, Qv3Clay, Qv4, QvDescription
                '计算抗拔
                AnalysisOfQb(DrillingID, i, ComputeLevels, Soils, LegParameter, SpudcanParameter, CalculateParameter)
            Next
            '穿刺风险评估
            AssessmentOfPunctureRisk(DrillingID, PressLimitValue, SpudcanParameter)
        Next
        '汇总计算深度结果
        CalculateDepthResult(False, PressLimitValue)
        MyDataSet.AcceptChanges()
        EsMessageReporter.ReportMessageFunction("公式法计算结束" & Chr(13), EsMessageType.Normal)
    End Sub
    Function GetQvDescription(ByVal OldDescription As String, ByVal NewDescription As String, ByVal QvSelectName As String, ByVal QvShowName As String, ByVal AddDescription As String) As String
        '删除旧对应备注
        Dim QvDescription As String = RemoveSelectQvDescription(OldDescription, QvShowName)
        '获得旧对应备注
        Dim TempOldQvDescription As String = GetSelectQvDescription(OldDescription, QvShowName, QvShowName)
        '获得新对应备注
        Dim TempNewQvDescription As String = GetSelectQvDescription(NewDescription, QvSelectName, QvShowName)
        '拼接新旧备注
        Return QvDescription & ";" & TempOldQvDescription & Chr(13) & AddDescription & Chr(13) & TempNewQvDescription
    End Function
    Function GetSelectQvDescription(ByVal Description As String, ByVal QvSelectName As String, ByVal QvShowName As String) As String
        Dim TempQvDescription As String = ""
        Dim TemTip As String() = Description.Split({QvSelectName}, StringSplitOptions.RemoveEmptyEntries)
        For k = 0 To TemTip.Count - 1
            If k = TemTip.Count - 1 Then
                TempQvDescription &= QvShowName & TemTip(k).Split({";"}, StringSplitOptions.RemoveEmptyEntries)(0)
            Else
                If Not TemTip(k).Contains(";") Then
                    TempQvDescription &= If(TemTip(k).StartsWith("="), QvShowName, "") & TemTip(k)
                End If
            End If
        Next
        Return TempQvDescription
    End Function
    Function RemoveSelectQvDescription(ByVal Description As String, ByVal QvSelectName As String) As String
        Dim TempQvDescription As String = ""
        Dim TemTip As String() = Description.Split({QvSelectName}, StringSplitOptions.RemoveEmptyEntries)
        For k = 0 To TemTip.Count - 1
            If k = TemTip.Count - 1 Then
                TempQvDescription &= TemTip(k).Split({";"}, StringSplitOptions.RemoveEmptyEntries)(1)
            Else
                If TemTip(k).Contains(";") Then
                    TempQvDescription &= TemTip(k)
                End If
            End If
        Next
        Return TempQvDescription
    End Function
    Function GetH(SoilPhi As Double, ByVal SpudcanB As Double) As Double
        Dim HDivideB As Double
        Dim SoilPhis() As Double = {20, 25, 30, 35, 40, 45, 48}
        Dim MultipleValues() As Double = {2.5, 3, 4, 5, 7, 9, 11}
        Dim Coeffs(6, 1) As Double
        For i = 0 To 6
            Coeffs(i, 0) = SoilPhis(i) / 180 * PI
            Coeffs(i, 1) = MultipleValues(i)
        Next
        HDivideB = GetCoeff(SoilPhi, Coeffs, 7)
        Return HDivideB * SpudcanB
    End Function
    Function GetS(SoilPhi As Double)
        Dim FigureCoefficientS As Double
        Dim SoilPhis() As Double = {0, 20, 25, 30, 35, 40, 45, 48} '内摩擦角φ=0时，s=1* *
        Dim Figures() As Double = {1, 1.12, 1.3, 1.6, 2.25, 3.45, 5.5, 7.6}
        Dim Coeffs(7, 1) As Double
        For i = 0 To 7
            Coeffs(i, 0) = SoilPhis(i) / 180 * PI
            Coeffs(i, 1) = Figures(i)
        Next
        FigureCoefficientS = GetCoeff(SoilPhi, Coeffs, 8)
        Return FigureCoefficientS
    End Function
    Function GetHOrS(SoilPhi As Double, Optional SpudcanB As Double = 0)
        Dim HDivideB, FigureCoefficientS As Double
        Dim SoilPhis() As Double = {20, 25, 30, 35, 40, 45, 48}
        Dim MultipleValues() As Double = {2.5, 3, 4, 5, 7, 9, 11}
        Dim Figures() As Double = {1.12, 1.3, 1.6, 2.25, 3.45, 5.5, 7.6}
        For i = 0 To SoilPhis.Count - 2
            Dim Phi1 As Double = SoilPhis(i) / 180 * PI
            Dim Phi2 As Double = SoilPhis(i + 1) / 180 * PI
            If SoilPhi >= Phi1 And SoilPhi <= Phi2 Then
                HDivideB = MultipleValues(i) + (SoilPhi - Phi1) * (MultipleValues(i + 1) - MultipleValues(i)) / (Phi2 - Phi1)
                FigureCoefficientS = Figures(i) + (SoilPhi - Phi1) * (Figures(i + 1) - Figures(i)) / (Phi2 - Phi1)
                Exit For
            End If
        Next
        If SpudcanB <> 0 Then
            Return HDivideB * SpudcanB
        Else
            Return FigureCoefficientS
        End If
    End Function
    Function GetCaculateParameter() As CalculateParameter
        Dim CalculateParameter As New CalculateParameter
        'Dim MyDataSet As DataSet = AnApplication.GetStructureKit.GetData
        Dim row As DataRow = MyDataSet.Tables("LS_CalculationParameter").Rows(0)
        CalculateParameter.DestinationLevel = row("DestinationLevel")
        CalculateParameter.NCalculatePoint = row("NCalculatePoint")
        CalculateParameter.CalculationMethod = row("CalculationMethod")
        CalculateParameter.MeshSize = row("MeshSize")
        CalculateParameter.DPType = row("DPType")
        CalculateParameter.DCoeff = row("DCoeff")
        CalculateParameter.KeepHistory = row("KeepHistory")
        CalculateParameter.IsBackFlow = row("IsBackFlow")
        CalculateParameter.AutoGetHc = row("AutoGetHc")
        CalculateParameter.Hc = row("Hc")
        CalculateParameter.Hc2 = row("Hc2")
        CalculateParameter.fbase = row("fbase")
        CalculateParameter.ftop = row("ftop")
        CalculateParameter.fleg = row("fleg")
        CalculateParameter.NBreakout = row("NBreakout")
        CalculateParameter.alpha = row("SoilCoarseCoeff")
        CalculateParameter.fb = row("fb")
        CalculateParameter.PressForce = row("PressForce")
        CalculateParameter.IsEquivalentToCircleSpudcan = row("IsEquivalentToCircleSpudcan")
        CalculateParameter.UnderWaterPhiSubtractValue = row("UnderWaterPhiSubtractValue")


        Return CalculateParameter
    End Function
    Function GetSoils(UnderWaterPhiSubtractValue As Double, DrillingID As Integer, SelectSingleDrilling As Boolean, Optional ByRef ErrorString As String = "", Optional ByVal Boats As Boolean = False) As List(Of SoilParameter) 'WaterWeight As Double,
        'Dim MyDataSet As DataSet = AnApplication.GetStructureKit.GetData
        Dim Soils As New List(Of SoilParameter)
        Dim SuInputType As Integer = MyDataSet.Tables("LS_Common").Rows(0).Item("SuInputType")
        Dim Rows() As DataRow = If(SelectSingleDrilling, MyDataSet.Tables("LS_LegSoilLayer").Select, MyDataSet.Tables("LS_SoilDrilling").Select("ID=" & DrillingID, "ID"))
        Dim SoilLevelIDDic As New Dictionary(Of Double, Integer)
        Dim TempErrorString As String = ""
        Dim SoilFilterString As String = If(Boats, " And DrillingID=" & DrillingID, "")
        Dim DrillingName As String = ""
        If SelectSingleDrilling Then
            DrillingName = Rows(0)("DrillingName")
            For Each row As DataRow In Rows
                SoilLevelIDDic.Add(row("TopLevel"), row("SoilID"))
            Next
        Else
            DrillingName = Rows(0)("Name")
            Dim SoilLayers() As String = Split(Rows(0)("SoilLayers"), ";")
            For Each Layer In SoilLayers
                If MyDataSet.Tables("LS_Soil").Select("Name='" & Layer.Split(",")(0) & "'" & SoilFilterString).Count = 0 Then
                    TempErrorString &= """" & DrillingName & """钻孔下的土层""" & Layer.Split(",")(0) & """在土层参数中未找到！" & vbCrLf
                Else
                    Dim SoilID As Integer = Integer.Parse(MyDataSet.Tables("LS_Soil").Select("Name='" & Layer.Split(",")(0) & "'" & SoilFilterString)(0)("ID"))
                    Dim TopLevel As Double = Val(Layer.Split(",")(1))
                    If SoilLevelIDDic.ContainsKey(TopLevel) Then
                        TempErrorString &= """" & DrillingName & """钻孔下的土层""" & Layer.Split(",")(0) & """的标高重复！" & vbCrLf
                    Else
                        SoilLevelIDDic.Add(TopLevel, SoilID)
                    End If
                End If
            Next
        End If
        If TempErrorString <> "" Then
            ErrorString &= TempErrorString
            Return Soils
        End If
        Dim I As Integer = 0
        For Each Level In SoilLevelIDDic.Keys
            I += 1
            Dim SoilRow As DataRow = MyDataSet.Tables("LS_Soil").Select("ID=" & SoilLevelIDDic(Level) & SoilFilterString)(0)
            Dim Soil As New SoilParameter
            Soil.Name = SoilRow("Name")
            Soil.SoilID = SoilLevelIDDic(Level)
            Soil.TopLevel = Level
            Soil.SoilType = SoilRow("Type")
            Soil.SuCurve.SetString(SoilRow("Su"))
            Soil.SuInputType = SuInputType
            Soil.Su0 = SoilRow("Su0")
            Soil.DSu = SoilRow("DSu")
            Soil.SuCurve.Reverse()
            Soil.Weight = SoilRow("UnderWaterWeight") ' - WaterWeight
            Soil.Phi = If(SoilRow("Type") = SoilType.Sand, (SoilRow("UnderWaterPhi") - UnderWaterPhiSubtractValue), SoilRow("UnderWaterPhi")) / 180 * PI



            Soil.BottomLevel = -10000
            If I > 1 Then
                Soils(Soils.Count - 1).BottomLevel = Soil.TopLevel
            End If
            Soils.Add(Soil)
            If Soil.SoilType = SoilType.Sand And Soil.Phi < 0 Or Soil.Phi >= 0.5 * PI Then
                ErrorString &= """" & DrillingName & """钻孔" & "下的土层""" & SoilRow("Name") & """的砂土内摩擦角未在范围内[0°,90°)" & vbCrLf
            End If
        Next
        Return Soils
    End Function
    Function GetSoils(UnderWaterPhiSubtractValue As Double) As List(Of SoilParameter) 'WaterWeight As Double

        'Dim MyDataSet As DataSet = AnApplication.GetStructureKit.GetData
        Dim Soils As New List(Of SoilParameter)
        Dim I As Integer = 0
        Dim Rows() As DataRow = MyDataSet.Tables("LS_LegSoilLayer").Select
        Dim SuInputType As Integer = MyDataSet.Tables("LS_Common").Rows(0).Item("SuInputType")
        For Each row As DataRow In Rows
            I += 1
            Dim SoilRow As DataRow = MyDataSet.Tables("LS_Soil").Select("ID=" & row("SoilID"))(0)
            Dim Soil As New SoilParameter
            Soil.SoilID = row("SoilID")
            Soil.SoilType = SoilRow("Type")
            Soil.SuCurve.SetString(SoilRow("Su"))
            Soil.SuInputType = SuInputType
            Soil.Su0 = SoilRow("Su0")
            Soil.DSu = SoilRow("DSu")
            Soil.SuCurve.Reverse()
            Soil.Weight = SoilRow("UnderWaterWeight") ' - WaterWeight
            Soil.Phi = If(SoilRow("Type") = SoilType.Sand, (SoilRow("UnderWaterPhi") - UnderWaterPhiSubtractValue) / 180 * PI, SoilRow("UnderWaterPhi"))
            Soil.TopLevel = row("TopLevel")
            Soil.BottomLevel = -10000
            'ATable.Columns("OnLegWeightReduceCoeff").DefaultValue = 1
            'ATable.Columns("OnLegStrenthengReduceCoeff").DefaultValue = 1
            'ATable.Columns("OnLegEReduceCoeff").DefaultValue = 1
            'ATable.Columns("OnLegMuReduceCoeff").DefaultValue = 1
            Soil.OnLegWeightReduceCoeff = SoilRow("OnLegWeightReduceCoeff")
            Soil.OnLegStrenthengReduceCoeff = SoilRow("OnLegStrenthengReduceCoeff")
            Soil.OnLegEReduceCoeff = SoilRow("OnLegEReduceCoeff")
            Soil.OnLegMuReduceCoeff = SoilRow("OnLegMuReduceCoeff")


            If I > 1 Then
                Soils(Soils.Count - 1).BottomLevel = Soil.TopLevel
            End If
            Soils.Add(Soil)
            If Soil.SoilType = SoilType.Sand And Soil.Phi < 0 Or Soil.Phi >= 0.5 * PI Then
                EsMessageReporter.ReportMessageFunction("土层""" & SoilRow("Name") & """的砂土内摩擦角未在范围内[0°,90°)", EsMessageType.Warning)
            End If
        Next
        Return Soils
    End Function
    Function GetComputeLevels(ByVal NCalculatePoint As Integer, ByVal Soils As List(Of SoilParameter), ByVal TopLevel As Double, ByVal BottomLevel As Double, Optional AddTopLevel As Boolean = True) As List(Of Double)
        Dim ComputeLevels As New List(Of Double)
        Dim TempComputeLevels As New List(Of Double)
        Dim DLevel0 As Double = (TopLevel - BottomLevel) / (NCalculatePoint - 1)
        Dim CurrentTopLevel As Double
        If AddTopLevel Then TempComputeLevels.Add(TopLevel)
        For j = 0 To Soils.Count - 1
            Dim Soil As SoilParameter = Soils(j)
            If Soil.BottomLevel < TopLevel Then
                CurrentTopLevel = Min(TopLevel, Soil.TopLevel)
                Dim N As Integer = Max((CurrentTopLevel - Max(BottomLevel, Soil.BottomLevel)) / DLevel0, 1)
                Dim DLevel As Double = (CurrentTopLevel - Max(BottomLevel, Soil.BottomLevel)) / N
                If DLevel > 0 Then
                    For i As Integer = 1 To N
                        TempComputeLevels.Add(Round(CurrentTopLevel - i * DLevel, 2))
                    Next
                End If
                '计算点加密
                If j = 0 And (Soil.SoilType = SoilType.Sand Or Soil.SoilType = SoilType.Both) And Soil.TopLevel - Soil.BottomLevel > 1 Then
                    N = Fix(CurrentTopLevel - Max(BottomLevel, Soil.BottomLevel))
                    DLevel = 1
                    If N > 0 Then
                        For i As Integer = 1 To N
                            If Not TempComputeLevels.Contains(Round(CurrentTopLevel - i * DLevel, 2)) Then TempComputeLevels.Add(Round(CurrentTopLevel - i * DLevel, 2))
                        Next
                    End If
                End If
            End If
        Next
        TempComputeLevels.Sort()
        For i = TempComputeLevels.Count - 1 To 0 Step -1
            ComputeLevels.Add(TempComputeLevels(i))
        Next
        Return ComputeLevels
    End Function
    Function GetAverageSoilValue(ByVal Soils As List(Of SoilParameter), ByVal FromLevel As Double, ByVal ToLevel As Double) As SoilParameter
        Dim SumSoil As New SoilParameter
        Dim H As Double = 0
        Dim SumH As Double = 0
        For Each ASoil In Soils
            If ASoil.BottomLevel < ToLevel And ASoil.TopLevel > FromLevel Then
                H = Min(ASoil.TopLevel, ToLevel) - Max(ASoil.BottomLevel, FromLevel)
                SumH += H
                SumSoil.Phi += H * ASoil.Phi
                SumSoil.Weight += H * ASoil.Weight
                SumSoil.Su0 += H * (ASoil.GetSu(Min(ASoil.TopLevel, ToLevel)) + ASoil.GetSu(Max(ASoil.BottomLevel, FromLevel))) / 2
            End If
        Next
        If SumH <> 0 Then

            SumSoil.Phi = SumSoil.Phi / SumH
            SumSoil.Weight = SumSoil.Weight / SumH
            SumSoil.Su0 = SumSoil.Su0 / SumH
        End If
        Return SumSoil
    End Function
    Function GetAverageSoilValue(ByVal Soils As List(Of SoilParameter), ByVal FromLevel As Double, ByVal ToLevel As Double, ByRef SumH As Double, Optional SelectSoilType As Integer = 99) As SoilParameter
        Dim SumSoil As New SoilParameter
        Dim H As Double = 0
        'Dim SumH As Double = 0
        SumH = 0
        For Each ASoil In Soils
            If ASoil.BottomLevel < ToLevel And ASoil.TopLevel > FromLevel Then
                If SelectSoilType = 99 Or ASoil.SoilType = SoilType.Both Or ASoil.SoilType = SelectSoilType Then
                    H = Min(ASoil.TopLevel, ToLevel) - Max(ASoil.BottomLevel, FromLevel)
                    SumH += H

                    SumSoil.Phi += H * ASoil.Phi
                    SumSoil.Weight += H * ASoil.Weight
                    SumSoil.Su0 += H * (ASoil.GetSu(Min(ASoil.TopLevel, ToLevel)) + ASoil.GetSu(Max(ASoil.BottomLevel, FromLevel))) / 2
                End If
            End If
        Next
        If SumH <> 0 Then

            SumSoil.Phi = SumSoil.Phi / SumH
            SumSoil.Weight = SumSoil.Weight / SumH
            SumSoil.Su0 = SumSoil.Su0 / SumH
        End If
        Return SumSoil
    End Function
    Sub GetHc(DrillingID As Integer, SpudcanParameter As SpudcanParameter， CalculateParameter As CalculateParameter, Soils As List(Of SoilParameter))
        Dim Hc, SpudcanB As Double
        SpudcanB = SpudcanParameter.GetSpudcanB()
        If CalculateParameter.AutoGetHc Then
            If Soils.Count = 0 Then
                Hc = GetHc_SingleLayer(SpudcanB, Soils(0))
            Else
                Hc = GetHc_multiLayer(SpudcanB, Soils)
            End If
            CalculateParameter.Hc = Hc
        End If
        Dim NewRow As DataRow
        NewRow = MyDataSet.Tables("LS_Holl").Rows.Add
        NewRow("DrillingID") = DrillingID
        NewRow("Hc") = Round(CalculateParameter.Hc, 2)
    End Sub
    Function GetHc_SingleLayer(ByVal B As Double, ByVal Soil As SoilParameter) As Double
        Dim S As Double = (Soil.GetSu0 / (Soil.Weight * B)) ^ (1 - Soil.GetDSu / Soil.Weight)
        Return B * (S ^ 0.55 - 0.25 * S)
    End Function
    Function GetHc_multiLayer(ByVal B As Double, ByVal Soils As List(Of SoilParameter)) As Double '迭代计算
        's(i,0)-距离顶面高度，s(i,1)-不排水强度，s(i,2)-重度
        '计算深度H以上加权值
        Dim Hc0, Hc1, S, T, TopLevel As Double
        Hc0 = GetHc_SingleLayer(B, Soils(0))
        Hc1 = 0
        TopLevel = Soils(0).TopLevel
        Do While Abs(Hc1 - Hc0) / Hc0 > 0.001
            Dim Soil As SoilParameter = GetAverageSoilValue(Soils, TopLevel - Hc0, TopLevel)
            S = Soils(0).GetSu0() / (Soil.Weight * B)
            Hc1 = B * (S ^ 0.55 - 0.25 * S)
            T = Hc0
            Hc0 = Hc1
            Hc1 = T
        Loop
        Return Hc0
    End Function
    Function GetLegParameter() As LegParameter
        'Dim MyDataSet As DataSet = AnApplication.GetStructureKit.GetData
        Dim LegParameter As New LegParameter
        Dim row As DataRow = MyDataSet.Tables("LS_Leg").Rows(0)

        LegParameter.Circumference = row("Circumference")
        LegParameter.Diameter = row("Diameter")
        LegParameter.Area = row("Area")
        'LegParameter.Volume = row("Volume")
        'LegParameter.Volume0 = row("Volume0")
        'LegParameter.TopLevel = row("TopLevel")
        'LegParameter.Weight = row("Weight") - row("Volume") * WaterWeight
        Return LegParameter
    End Function
    Function GetSpudcanParameter() As SpudcanParameter
        'Dim MyDataSet As DataSet = AnApplication.GetStructureKit.GetData
        Dim SpudcanParameter As New SpudcanParameter
        Dim row As DataRow = MyDataSet.Tables("LS_Spudcan").Rows(0)
        SpudcanParameter.Area = row("Area")
        SpudcanParameter.Circumference = row("Circumference")
        SpudcanParameter.Diameter = row("Diameter")
        SpudcanParameter.Volume = row("Volume")
        SpudcanParameter.Weight = row("Weight") - 9.8 * If(MyDataSet.Tables("LS_CalculationParameter").Rows(0)("IsSealed"), row("Volume") * 1, 0)  'kN，水密度1000kg/m3
        SpudcanParameter.L = row("L")
        SpudcanParameter.B = row("B")
        SpudcanParameter.B1 = row("B")

        Dim Parameters() As String = Split(row("Parameter"), ",")
        Dim H1 As Double = Val(GetParameter(Parameters, "H1"))
        Dim H2 As Double = Val(GetParameter(Parameters, "H2"))
        Dim H3 As Double = Val(GetParameter(Parameters, "H3"))
        SpudcanParameter.Ht = H2

        SpudcanParameter.H1 = 0
        SpudcanParameter.H2 = H1
        SpudcanParameter.H3 = H2
        SpudcanParameter.H4 = H3
        SpudcanParameter.ShapeType = row("ShapeType")
        Return SpudcanParameter
    End Function
    Shared Function GetParameter(Parameters() As String, ParameterName As String) As String
        For Each Parameter In Parameters
            If Parameter.Contains("=") Then
                Dim Strs() As String = Split(Parameter, "=")
                If Strs(0) = ParameterName Then
                    Return Strs(1)
                End If

            End If
        Next
        Return "0"
    End Function
    Function GetIsDownSoilTypeExtra(ByVal Level As Double, SpudcanB As Double, ByVal Soils As List(Of SoilParameter)) As Boolean
        Dim TypeList As New List(Of Integer)
        For i As Integer = 0 To Soils.Count - 1
            If Soils(i).BottomLevel < Level And Soils(i).TopLevel > Level - 0.5 * SpudcanB Then
                If TypeList.Contains(Soils(i).SoilType) = False Then
                    TypeList.Add(Soils(i).SoilType)
                End If
            End If
        Next
        If TypeList.Count <= 2 Then
            Return False
        Else
            Return True
        End If
    End Function
    Function GetDownSandLayersNextLevel(ByVal Level As Double, ByVal Soils As List(Of SoilParameter), Optional ByRef MergeSandSoil As Boolean = False) As Double
        Dim SandNextLevell As Double = 10 ^ 10
        For i As Integer = 0 To Soils.Count - 1
            If Soils(i).TopLevel = Level And Soils(i).SoilType = SoilType.Clay Then
                Return SandNextLevell
            End If
            If Soils(i).BottomLevel < Level Then
                If Soils(i).SoilType = SoilType.Clay Then
                    Return SandNextLevell
                Else
                    SandNextLevell = Soils(i).BottomLevel
                    MergeSandSoil = True
                End If
            End If
        Next
        Return SandNextLevell
    End Function
    Function GetIsSameUpSoilType(ByVal Level As Double, ByVal Soils As List(Of SoilParameter)) As Boolean
        Dim TypeList As New List(Of Integer)
        For I As Integer = 0 To Soils.Count - 1
            If Soils(I).TopLevel > Level Then
                If TypeList.Contains(Soils(I).SoilType) = False Then
                    TypeList.Add(Soils(I).SoilType)
                End If
            End If
        Next
        If TypeList.Count <= 1 Then
            Return True
        Else
            Return False
        End If
    End Function
    Function GetSoil(ByVal Level As Double, ByVal Soils As List(Of SoilParameter)) As SoilParameter
        If Soils(0).TopLevel < Level Then
            Return Soils(0)
        End If
        Dim Index As Integer = Soils.Count - 1
        For I As Integer = 0 To Soils.Count - 1
            If Soils(I).TopLevel >= Level And Soils(I).BottomLevel < Level Then
                Index = I
                Exit For
            End If
        Next
        Return Soils(Index)
    End Function
    Function GetP0(ByVal Level As Double, ByVal Soils As List(Of SoilParameter)) As Double
        Dim P As Double = 0
        For Each ASoil In Soils
            If ASoil.TopLevel > Level Then
                If ASoil.BottomLevel > Level Then
                    P += (ASoil.TopLevel - ASoil.BottomLevel) * ASoil.Weight
                Else
                    P += (ASoil.TopLevel - Level) * ASoil.Weight
                End If
            End If
        Next
        Return P
    End Function
    '除了穿刺破坏(砂土穿刺软粘土)模式,考虑回流的承载力计算
    Sub GetBackFlowResult_Other(ByRef Qv As Double, ByVal Level As Double, ByVal LegParameter As LegParameter, ByVal SpudcanParameter As SpudcanParameter, ByVal Soils As List(Of SoilParameter), ByVal Hc As Double, Optional ByRef Description As String = "")
        GetBackFlowResult_PunchSand(Qv, Level, LegParameter, SpudcanParameter, Soils, Hc, Description)
    End Sub
    Sub GetBackFlowResult_PunchSand(ByRef Qv As Double, ByVal Level As Double, ByVal LegParameter As LegParameter, ByVal SpudcanParameter As SpudcanParameter, ByVal Soils As List(Of SoilParameter), ByVal Hc As Double, Optional ByRef Description As String = "")
        Dim D As Double = Soils(0).TopLevel - Level
        Dim AverageSoil As SoilParameter = GetAverageSoilValue(Soils, Level, Soils(0).TopLevel)
        Dim TempValue As Double = AverageSoil.Weight * (SpudcanParameter.Area * (D - Hc) - LegParameter.Area * (D - Hc - SpudcanParameter.H3 - SpudcanParameter.H4) - SpudcanParameter.Volume) 'I=(D - Hc - ((SpudcanParameter.Volume - Vd) / SpudcanParameter.Area),桩靴上部覆土高度，反馈-当砂土层覆盖在软粘土层上时（穿刺破坏）计算问题及修改.pptx
        Qv -= Max(TempValue, 0)
        Description &= "-" & If(TempValue < 0, "0", Round(AverageSoil.Weight, 3) & "×[" & Round(SpudcanParameter.Area) & "×(" & Round(D, 3) & "-" & Round(Hc, 3) & ")-" & Round(LegParameter.Area, 3) & "×(" & Round(D, 3) & "-" & Round(Hc, 3) & "-" & If(SpudcanParameter.H4 = 0, Round(SpudcanParameter.H3, 3), "(" & Round(SpudcanParameter.H3, 3) & "+" & Round(SpudcanParameter.H4, 3) & ")") & ")-" & Round(SpudcanParameter.Volume, 3) & "]")
    End Sub
    Function GetQV_Clay(ByVal B As Double, ByVal Level As Double, ByVal LegParameter As LegParameter, ByVal SpudcanParameter As SpudcanParameter, ByVal Soil As SoilParameter, ByVal Soils As List(Of SoilParameter), ByVal IsBackFlow As Boolean, Optional Hc As Double = 0, Optional ByRef Description As String = "", Optional QvName As String = "", Optional DispersionL As Double = 0) As Double '按照黏土计算承载力
        Dim Qv As Double
        Dim Nc As Double = 5.14
        Dim Sc As Double
        Dim D As Double = Soils(0).TopLevel - Level
        Dim Dc As Double = Min(1 + 0.2 * D / B, 1.5)
        Dim Nq As Double = 1
        Dim Nc_Sc_Dc As Double
        Dim OutOfRange As Boolean = False
        Dim Coeffs(,) As Double = {{0, 6}, {0.1, 6.3}, {0.25, 6.6}, {0.5, 7}, {1.0, 7.7}, {2.5, 9}}
        Nc_Sc_Dc = GetCoeff(D / B, Coeffs, 5, OutOfRange)
        If SpudcanParameter.ShapeType = 1 Or OutOfRange Then
            Sc = 1 + (Nq / Nc) * (B / If(DispersionL <> 0, DispersionL, SpudcanParameter.L))
            Nc_Sc_Dc = Nc * Sc * Dc
        End If
        '考虑到砂土穿刺粘土的扩散，砂土下粘土的常规承载力中面积按扩散面积计算
        Dim SpudcanA As Double = If(DispersionL <> 0, If(SpudcanParameter.ShapeType = 1, B * DispersionL, PI * B ^ 2 / 4), SpudcanParameter.Area)
        Qv = (Soil.GetSu(Level) * Nc_Sc_Dc + GetP0(Level, Soils)) * SpudcanA
        Description &= If(QvName = "", "Qv", QvName) & "=(Su" & If(SpudcanParameter.ShapeType = 1 Or OutOfRange, "Ncscdc", "(Ncscdc)") & "+p'0)A" & If(IsBackFlow, "-γ'[A(D-Hc)-Al(D-Hc-" & If(SpudcanParameter.H4 = 0, "Ht", "(H2+H3)") & ")-V]" & Chr(13), Chr(13)) '-FOA+γ'V=γ’[As(D-Hc)-Al(D-Hc-Ht)-V]
        Description &= If(QvName = "", "Qv", QvName) & "=(" & Round(Soil.GetSu(Level), 3) & "×" & If(SpudcanParameter.ShapeType = 1 Or OutOfRange, Round(Nc, 3) & "×" & Round(Sc, 3) & "×" & Round(Dc, 3), "(" & Round(Nc_Sc_Dc, 3) & ")") & "+" & Round(GetP0(Level, Soils), 3) & ")×" & Round(SpudcanA, 3)
        If IsBackFlow Then GetBackFlowResult_Other(Qv, Level, LegParameter, SpudcanParameter, Soils, Hc, Description)
        Description &= "=" & Round(Qv, 3) & ";"
        Return Qv
    End Function
    Function GetCoeff(ByVal V As Double, Coeffs(,) As Double, N As Integer, Optional ByRef OutOfRange As Boolean = False) As Double
        If V > Coeffs(N - 1, 0) Then
            OutOfRange = True
            Return Coeffs(N - 1, 1)
        Else
            If V < Coeffs(0, 0) Then
                OutOfRange = True
                Return Coeffs(0, 1)
            Else
                For i = 0 To N - 2
                    If V >= Coeffs(i, 0) And V <= Coeffs(i + 1, 0) Then
                        Return Coeffs(i, 1) + (Coeffs(i + 1, 1) - Coeffs(i, 1)) * (V - Coeffs(i, 0)) / (Coeffs(i + 1, 0) - Coeffs(i, 0))
                    End If
                Next
            End If
        End If
        Return Coeffs(0, 1)
    End Function
    Function GetQV_Sand(ByVal IsEquivalentToCircleSpudcan As Boolean, ByVal B As Double, ByVal Level As Double, ByVal NextSoilLevel As Double, ByVal LegParameter As LegParameter, ByVal SpudcanParameter As SpudcanParameter, ByVal Soil As SoilParameter, ByVal Soils As List(Of SoilParameter), ByVal IsBackFlow As Boolean, Optional Hc As Double = 0, Optional ByRef Description As String = "", Optional QvName As String = "") As Double '按照砂土土计算承载力
        Dim Qv As Double
        Dim D As Double = Soils(0).TopLevel - Level '插深,桩靴最大截面处下部到海床面的距离
        Dim TheAverageSoil As SoilParameter = Soil
        If NextSoilLevel < Soil.BottomLevel Then
            TheAverageSoil = GetAverageSoilValue(Soils, NextSoilLevel, Level)
        End If
        Dim SoilWeight As Double = TheAverageSoil.Weight
        Dim Dgamma As Double = 1
        Dim Nq, Ngamma As Double

        Dim OutOfRange As Boolean = False
        Dim SoilPhis() As Double = {20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40}
        Dim Ngammas() As Double = {2.4, 2.9, 3.5, 4.2, 5.1, 6.1, 7.3, 8.8, 10.6, 12.8, 15.5, 18.8, 22.9, 27.9, 34.1, 41.9, 51.6, 63.7, 79.1, 98.7, 123.7}
        Dim Nqs() As Double = {9.6, 10.9, 12.4, 14.1, 16.1, 18.4, 21.1, 24.2, 27.9, 32.2, 37.2, 43.2, 50.3, 58.7, 68.7, 80.8, 95.4, 113, 134.4, 160.5, 192.7}
        Dim PhiNgammas(SoilPhis.Count - 1, 1), PhiNqs(SoilPhis.Count - 1, 1) As Double 'SoilPhis.Count=21
        For i = 0 To SoilPhis.Count - 1
            PhiNgammas(i, 0) = SoilPhis(i) / 180 * PI
            PhiNgammas(i, 1) = Ngammas(i)
            PhiNqs(i, 0) = SoilPhis(i) / 180 * PI
            PhiNqs(i, 1) = Nqs(i)
        Next
        Nq = GetCoeff(TheAverageSoil.Phi, PhiNqs, SoilPhis.Count, OutOfRange)
        If (SpudcanParameter.ShapeType = 1 And Not IsEquivalentToCircleSpudcan) Or OutOfRange Then
            Nq = Exp(PI * Tan(TheAverageSoil.Phi)) * (Tan(PI / 4 + TheAverageSoil.Phi / 2)) ^ 2
        End If
        Ngamma = GetCoeff(TheAverageSoil.Phi, PhiNgammas, SoilPhis.Count, OutOfRange)
        If (SpudcanParameter.ShapeType = 1 And Not IsEquivalentToCircleSpudcan) Or OutOfRange Then
            Ngamma = 2 * (Nq + 1) * Tan(TheAverageSoil.Phi)
        End If

        Dim P0 As Double = GetP0(Level, Soils) '插深D范围内的上覆土压力 
        Dim dq As Double = If(D / B <= 1, 1 + 2 * Tan(TheAverageSoil.Phi) * (1 - Sin(TheAverageSoil.Phi)) ^ 2 * (D / B), 1 + 2 * Tan(TheAverageSoil.Phi) * (1 - Sin(TheAverageSoil.Phi)) ^ 2 * Atan(D / B))
        Qv = (SoilWeight * Dgamma * Ngamma * B / 2 + P0 * dq * Nq) * SpudcanParameter.Area
        Dim Sg, Sq As Double
        If Not IsEquivalentToCircleSpudcan Then
            Sg = 1 - 0.4 * (B / SpudcanParameter.L)
            Sq = 1 + Tan(TheAverageSoil.Phi) * (B / SpudcanParameter.L)
            Qv = (SoilWeight * Dgamma * Ngamma * B / 2 * Sg + P0 * dq * Nq * Sq) * SpudcanParameter.Area
        End If
        Description &= If(QvName = "", "Qv", QvName) & If(IsEquivalentToCircleSpudcan, "=(γ'dγNγB/2+p'0dqNq)A", "=(γ'dγsγNγB/2+p'0dqsqNq)A") & If(IsBackFlow, "-γ'[A(D-Hc)-Al(D-Hc-" & If(SpudcanParameter.H4 = 0, "Ht", "(H2+H3)") & ")-V]" & Chr(13), Chr(13)) '-FOA+γ'V=γ’[As(D-Hc)-Al(D-Hc-Ht)-V]
        Description &= If(QvName = "", "Qv", QvName) & "=(" & SoilWeight & "×" & Dgamma & If(IsEquivalentToCircleSpudcan, "", "×" & Round(Sg, 3)) & "×" & Round(Ngamma, 3) & "×" & Round(B, 3) & "/" & 2 &
            "+" & Round(P0, 3) & "×" & Round(dq, 3) & If(IsEquivalentToCircleSpudcan, "", "×" & Round(Sq, 3)) & "×" & Round(Nq, 3) & ")×" & Round(SpudcanParameter.Area, 3)
        If IsBackFlow Then GetBackFlowResult_Other(Qv, Level, LegParameter, SpudcanParameter, Soils, Hc, Description)
        Description &= "=" & Round(Qv, 3) & ";"
        Return Qv
    End Function
    Function GetQV_Squeeze(ByVal IsEquivalentToCircleSpudcan As Boolean, ByVal B As Double, ByVal Level As Double, ByVal NextLevel As Double, ByVal LegParameter As LegParameter, ByVal SpudcanParameter As SpudcanParameter, ByVal Soil As SoilParameter, ByVal Soils As List(Of SoilParameter), ByVal IsBackFlow As Boolean, Optional Hc As Double = 0, Optional ByRef Description As String = "", Optional QvName As String = "") As Double '按照挤出计算承载力
        Dim NormalString As String = "挤出破坏模式："
        If Soil.SoilType = SoilType.Clay Or Soil.SoilType = SoilType.Both Then
            Dim D As Double = Soils(0).TopLevel - Level
            Dim T As Double = Max(Level - Soil.BottomLevel, 0.01)
            Dim Qv As Double
            '挤出破坏模式计算的竖向承载力下限值为软土的常规破坏模式承载力计算结果，上限值为下层硬土的承载力计算结果
            '获得下限值
            Dim TempDescriptionMin As String = ""
            Dim Qv_Min As Double = GetQV_Clay(B, Level, LegParameter, SpudcanParameter, Soil, Soils, IsBackFlow, Hc, TempDescriptionMin, QvName)
            If B >= 3.45 * T * (1 + 1.025 * D / B) And D / B <= 2.5 Then
                Dim Ass As Double = 5
                Dim Bss As Double = 0.33
                Dim P0 As Double = GetP0(Level, Soils)
                Qv = SpudcanParameter.Area * ((Ass + Bss * B / T + 1.2 * D / B) * Soil.GetSu(Level) + P0)
                Dim TempDescription As String = If(QvName = "", "Qv", QvName) & "=A{(αs+bsB/T+1.2D/B)Su+p'0" & If(IsBackFlow, "-γ'[A(D-Hc)-Al(D-Hc-" & If(SpudcanParameter.H4 = 0, "Ht", "(H2+H3)") & ")-V]" & Chr(13), Chr(13)) '-FOA+γ'V=γ’[As(D-Hc)-Al(D-Hc-Ht)-V]
                TempDescription &= If(QvName = "", "Qv", QvName) & "=" & Round(SpudcanParameter.Area, 3) & "×((" & Ass & "+" & Bss & "×" & Round(B, 3) & "/" & Round(T, 3) & "+" & 1.2 & "×" & D & "/" & Round(B, 3) & ")×" & Round(Soil.GetSu(Level), 3) & "+" & Round(P0, 3) & ")"
                If IsBackFlow Then
                    GetBackFlowResult_Other(Qv, Level, LegParameter, SpudcanParameter, Soils, Hc, TempDescription)
                End If
                TempDescription &= "=" & Round(Qv, 3) & ";"
                If Qv < Qv_Min Then
                    NormalString &= "挤出破坏结果Qv=Max(挤出Qv(" & Qv & ")，常规Qv(" & Qv_Min & ")"
                    Qv = Qv_Min
                    Description &= TempDescription & Chr(13) & "Qv=Max(Qv，常规Qv)" & Chr(13) & TempDescriptionMin
                    EsMessageReporter.ReportMessageFunction(NormalString, EsMessageType.Normal)
                Else
                    Description &= TempDescription
                    '上限值待下层土计算完成后进行比对* *20240429
                End If
                Return Qv
            Else
                '当挤出条件不满足时，按常规破坏模式计算 
                Qv = Qv_Min
                Description &= TempDescriptionMin
                NormalString &= "T=" & Round(T, 2) & "，D=" & Round(D, 2) & "，B=" & Round(B, 2) & "，不满足挤出破坏条件（B≥3.45T（1+1.025D/B），且D/B≤2.5），按常规破坏模式计算"
                EsMessageReporter.ReportMessageFunction(NormalString, EsMessageType.Normal)
                Return Qv
                'Description &= If(QvName = "", "Qv", QvName) & "未计算;"
                'EsMessageReporter.ReportMessageFunction("T=" & Round(T, 2) & "，D=" & Round(D, 2) & "，B=" & Round(B, 2) & "，不满足挤出破坏条件：B≥3.45T（1+1.025D/B），且D/B≤2.5", EsMessageType.Normal)
                'Return 10 ^ 10
            End If
        Else
            Description &= If(QvName = "", "Qv", QvName) & "未计算;"
            NormalString &= "持力层土的土类型为砂土，不满足挤出破坏条件：持力层土承载力小于持力+1层土承载力，即上软下硬"
            EsMessageReporter.ReportMessageFunction(NormalString, EsMessageType.Normal)
            Return 10 ^ 10
        End If
    End Function
    Function GetQV_MultiLayer(ByVal B As Double, ByVal Level As Double, ByVal LegParameter As LegParameter, ByVal SpudcanParameter As SpudcanParameter, ByVal Soil As SoilParameter, ByVal Soils As List(Of SoilParameter), ByVal IsBackFlow As Boolean, Optional Hc As Double = 0, Optional ByRef Description As String = "", Optional QvName As String = "") As Double '按照砂土土计算承载力
        Dim Qv As Double
        Dim SoilWeight As Double = Soil.Weight
        Dim ig, iq, ic As Double
        Dim Nq, Ngamma, Nc, Sg, Sq, Sc As Double
        Dim P0 As Double = GetP0(Level, Soils) '插深D范围内的上覆土压力
        ig = 1
        iq = 1
        ic = 1
        Sg = 1 - 0.4 * (B / SpudcanParameter.L)
        Sq = 1 + Tan(Soil.Phi) * (B / SpudcanParameter.L)
        Nq = Exp(PI * Tan(Soil.Phi)) * (Tan(PI / 4 + Soil.Phi / 2)) ^ 2
        Ngamma = 2 * (Nq + 1) * Tan(Soil.Phi)
        If Soil.Phi = 0 Then
            Description &= If(QvName = "", "Qv", QvName) & "未计算;"
            EsMessageReporter.ReportMessageFunction("分层土破坏模式：持力层土的摩擦角为0，无法计算承载力修正系数Nc，不进行分层土破坏模式计算", EsMessageType.Normal)
            Return 10 ^ 10
        End If
        Nc = (Nq - 1) / Tan(Soil.Phi)
        Sc = 1 + (Nq / Nc) * (B / SpudcanParameter.L)
        Qv = (0.5 * SoilWeight * B * Ngamma * Sg * ig + P0 * Nq * Sq * iq + Soil.GetSu(Level) * Nc * Sc * ic) * SpudcanParameter.Area
        Description &= If(QvName = "", "Qv", QvName) & "=(0.5γ'BNγsγiγ+p'0Nqsqiq+suNcscic)A" & If(IsBackFlow, "-γ'[A(D-Hc)-Al(D-Hc-" & If(SpudcanParameter.H4 = 0, "Ht", "(H2+H3)") & ")-V]" & Chr(13), Chr(13)) '-FOA+γ'V=γ’[As(D-Hc)-Al(D-Hc-Ht)-V]
        Description &= If(QvName = "", "Qv", QvName) & "=(" & 0.5 & "×" & SoilWeight & "×" & Round(B, 3) & "×" & Round(Ngamma, 3) & "×" & Round(Sg, 3) & "×" & ig & "+" & Round(P0, 3) & "×" & Round(Nq, 3) & "×" & Round(Sq, 3) & "×" & iq & "+" & Round(Soil.GetSu(Level), 3) & "×" & Round(Nc, 3) & "×" & Round(Sc, 3) & "×" & ic & ")×" & Round(SpudcanParameter.Area, 3)
        If IsBackFlow Then GetBackFlowResult_Other(Qv, Level, LegParameter, SpudcanParameter, Soils, Hc, Description)
        Description &= "=" & Round(Qv, 3) & ";"
        Return Qv
    End Function
    Function GetQV_Punch_Clay(ByVal B As Double, ByVal Level As Double, ByVal NextLevel As Double, ByVal LegParameter As LegParameter, ByVal SpudcanParameter As SpudcanParameter, ByVal Soil As SoilParameter, ByVal Soils As List(Of SoilParameter), ByVal IsBackFlow As Boolean, Optional Hc As Double = 0, Optional ByRef Description As String = "", Optional QvName As String = "") As Double '按照穿刺计算承载力
        '需要对下面的所有土层验算穿刺
        Dim Qv_Clay, Qv As Double
        Dim TempDes As String = ""
        Dim D As Double = Soils(0).TopLevel - Level
        Dim H As Double
        Dim Suto, Subo As Double
        Dim Nc As Double = 5.14
        Dim Nq As Double = 1
        Dim Nc_Sc As Double
        If SpudcanParameter.ShapeType = 0 Then
            Nc_Sc = 6
        Else
            Dim Sc As Double = 1 + (Nq / Nc) * (B / SpudcanParameter.L)
            Nc_Sc = Nc * Sc
        End If
        '对下部软粘土层进行循环计算求得最小承载力
        Suto = Soil.GetSu(Level)
        Dim P0 As Double = GetP0(Level, Soils) '插深D范围内的上覆土压力
        Dim BottomSoil As SoilParameter = GetSoil(NextLevel, Soils)
        '常规破坏模式
        Dim TempDescriptionMax As String = ""
        Qv_Clay = GetQV_Clay(B, Level, LegParameter, SpudcanParameter, Soil, Soils, IsBackFlow, Hc, TempDescriptionMax, QvName)
        Dim NormalString As String = "穿刺破坏模式："
        If Level = NextLevel Or BottomSoil.SoilType = SoilType.Sand Then
            If Level = NextLevel Then
                Qv = Qv_Clay
                Description &= TempDescriptionMax
                NormalString &= Level & "为土层最底端，按常规破坏模式计算"
            Else
                Qv = 10 ^ 10
                TempDes = If(QvName = "", "Qv", QvName) & "未计算;"
                Description &= TempDes
                NormalString &= "持力+1层土的土类型为砂土，不满足穿刺破坏条件：持力层土承载力大于持力+1层土承载力，即上硬下软"
            End If
            EsMessageReporter.ReportMessageFunction(NormalString, EsMessageType.Normal)
        Else
            H = Level - NextLevel
            Subo = BottomSoil.GetSu(NextLevel)
            Qv = SpudcanParameter.Area * (3 * H / B * Suto + Nc_Sc * (1 + 0.2 * (D + H) / B) * Subo + P0)
            TempDes &= If(QvName = "", "Qv", QvName) & "=A[3H/BSu,t+(Ncsc)(1+0.2(D+H)/B)Su,b+p'0)]" & If(IsBackFlow, "-γ'[A(D-Hc)-Al(D-Hc-" & If(SpudcanParameter.H4 = 0, "Ht", "(H2+H3)") & ")-V]" & Chr(13), Chr(13)) '-FOA+γ'V=γ’[As(D-Hc)-Al(D-Hc-Ht)-V]
            TempDes &= If(QvName = "", "Qv", QvName) & "=" & Round(SpudcanParameter.Area, 3) & "×(" & 3 & "×" & Round(H, 3) & "/" & Round(B, 3) & "×" & Suto & "+(" & Round(Nc_Sc, 3) & ")×(1 + 0.2 ×(" & D & "+" & Round(H, 3) & ")/" & Round(B, 3) & ")×" & Subo & "+" & Round(P0, 3) & ")"
            If IsBackFlow Then GetBackFlowResult_Other(Qv, Level, LegParameter, SpudcanParameter, Soils, Hc, TempDes)
            TempDes &= "=" & Round(Qv, 3) & ";"
            '取常规破坏模式和穿刺破坏模式的最小值
            If Qv <= Qv_Clay Then
                Description &= TempDes
            Else
                NormalString &= "穿刺破坏结果Qv=Min(常规Qv(" & Qv_Clay & "),穿刺Qv(" & Qv & ")"
                Qv = Qv_Clay
                Description &= TempDescriptionMax
                EsMessageReporter.ReportMessageFunction(NormalString, EsMessageType.Normal)
            End If
        End If
        'For i As Integer = 0 To Levels.Count - 1
        '    Dim TempDes1 As String = ""
        '    H = Level - Levels(i)
        '    Dim BottomSoil As SoilParameter = GetSoil(Levels(i), Soils)
        '    If BottomSoil.SoilType = SoilType.Clay Or BottomSoil.SoilType = SoilType.Both Then
        '        Subo = BottomSoil.GetSu(Levels(i))
        '        Qv = SpudcanParameter.Area * (3 * H / B * Suto + Nc_Sc * (1 + 0.2 * (D + H) / B) * Subo + P0)
        '        TempDes1 &= If(QvName = "", "Qv", QvName) & "=A[3H/BSu,t+(Ncsc)(1+0.2(D+H)/B)Su,b+p'0)]" & If(IsBackFlow, "-γ'((D-Hc)A-V)" & Chr(13), Chr(13))
        '        TempDes1 &= If(QvName = "", "Qv", QvName) & "=" & Round(SpudcanParameter.Area, 3) & "×(" & 3 & "×" & Round(H, 3) & "/" & Round(B, 3) & "×" & Suto & "+(" & Round(Nc_Sc, 3) & ")×(1 + 0.2 ×(" & D & "+" & Round(H, 3) & ")/" & Round(B, 3) & ")×" & Subo & "+" & Round(P0, 3) & ")"
        '        If IsBackFlow Then GetBackFlowResult_Other(Qv, Level, SpudcanParameter, Soils, Hc, TempDes1)
        '        TempDes1 &= "=" & Round(Qv, 3) & ";"
        '    Else
        '        Qv = 10 ^ 10
        '        TempDes1 = If(QvName = "", "Qv", QvName) & "未计算;"
        '    End If
        '    If i = 0 Then
        '        MinQv = Qv
        '        TempDes = TempDes1
        '    Else
        '        MinQv = Min(MinQv, Qv)
        '        If MinQv = Qv Then TempDes = TempDes1
        '    End If
        'Next
        Return Qv 'MinQv
    End Function
    Function GetQV_Punch_Sand(ByVal IsEquivalentToCircleSpudcan As Boolean, ByVal B As Double, ByVal Level As Double, ByVal NextLevel As Double, ByVal LegParameter As LegParameter, ByVal SpudcanParameter As SpudcanParameter, ByVal Soil As SoilParameter, ByVal Soils As List(Of SoilParameter), ByVal IsBackFlow As Boolean, Optional Hc As Double = 0, Optional ByRef Description As String = "", Optional QvName As String = "") As Double '按照穿刺计算承载力
        '需要对下面的所有土层验算穿刺
        'Dim Qvb = GetQV_Sand(Level, LegParameter, SpudcanParameter, Soil, Soils, IsBackFlow)
        Dim Qv_Sand, Qv As Double
        Dim D As Double = Soils(0).TopLevel - Level
        Dim H As Double '桩靴底(插深D处往下)到软弱土层的距离
        Dim P0 As Double = GetP0(Level, Soils) '插深D范围内的上覆土压力
        Dim Ks As Double
        Dim TheAverageSoil As SoilParameter = Soil
        If NextLevel < Soil.BottomLevel Then
            TheAverageSoil = GetAverageSoilValue(Soils, NextLevel, Level)
        End If
        Dim BottomSoil As SoilParameter = GetSoil(NextLevel, Soils)
        '常规破坏模式
        Dim TempDesSand As String = ""
        Qv_Sand = GetQV_Sand(IsEquivalentToCircleSpudcan, B, Level, NextLevel, LegParameter, SpudcanParameter, TheAverageSoil, Soils, IsBackFlow, Hc, TempDesSand, QvName)
        Dim NormalString As String = "穿刺破坏模式："
        If Level = NextLevel Or BottomSoil.SoilType = SoilType.Sand Then
            If Level = NextLevel Then
                Qv = Qv_Sand
                Description &= TempDesSand
                NormalString &= Level & "为土层最底端，按常规破坏模式计算"
                EsMessageReporter.ReportMessageFunction(NormalString, EsMessageType.Normal)
            Else
                NormalString &= "计算高程=" & Level & "处，多个砂土合并层的底部无软土层，不进行穿刺破坏模式计算!"
                EsMessageReporter.ReportMessageFunction(NormalString, EsMessageType.Normal)
            End If
        Else
            H = Level - NextLevel
            Dim TempDes As String = ""
            Dim TempDesClay As String = ""
            Dim QV1 As Double = GetQV_Sand(IsEquivalentToCircleSpudcan, B, Level, NextLevel, LegParameter, SpudcanParameter, TheAverageSoil, Soils, False, 0, TempDes, "Qsand")
            Dim QV2 As Double = GetQV_Clay(SpudcanParameter.GetSpudcanB, NextLevel, LegParameter, SpudcanParameter, BottomSoil, Soils, False, 0, TempDesClay, "Qclay")
            Dim Coeff As Double = QV2 / QV1
            Dim Weight As Double = TheAverageSoil.Weight
            TempDes = TempDes.TrimEnd(";"c) & Chr(13)
            TempDesClay = TempDesClay.TrimEnd(";"c)
            TempDes &= TempDesClay & Chr(13)
            TempDes &= "φ" & If(TheAverageSoil.Phi = 25 / 180 * PI, "=", If(TheAverageSoil.Phi < 25 / 180 * PI, "＜", "＞")) & "25°，Qclay/Qsand=" & Round(Coeff, 3) & If(Coeff = 0.1, "=", If(Coeff < 0.1, "＜", "＞")) & "0.1" & Chr(13)
            If TheAverageSoil.Phi < 25 / 180 * PI Or Coeff < 0.1 Then '20230802
                Dim TempDesClayB As String = ""
                Dim ns As Double = 3
                Dim DispersionB As Double = If(SpudcanParameter.ShapeType = 0, B, SpudcanParameter.GetSpudcanB) + 2 * H / ns
                Dim DispersionL As Double = If(SpudcanParameter.ShapeType = 0, SpudcanParameter.L, SpudcanParameter.L + 2 * H / ns)
                Dim W As Double = If(SpudcanParameter.ShapeType = 0, 0.25 * PI * DispersionB ^ 2, DispersionB * DispersionL) * H * Weight
                QV2 = GetQV_Clay(DispersionB, NextLevel, LegParameter, SpudcanParameter, BottomSoil, Soils, False, 0, TempDesClayB, "Qv,b", DispersionL)
                TempDes &= TempDesClayB.TrimEnd(";"c) & Chr(13)
                TempDes &= If(QvName = "", "Qv", QvName) & "=Qv,b-W" & If(IsBackFlow, "-γ'[A(D-Hc)-Al(D-Hc-" & If(SpudcanParameter.H4 = 0, "Ht", "(H2+H3)") & ")-V]" & Chr(13), Chr(13)) 'Aγ'I=γ’[As(D-Hc)-Al(D-Hc-Ht)V]20240823
                TempDes &= If(QvName = "", "Qv", QvName) & "=" & Round(QV2, 3) & "-" & Round(W, 3)
                Qv = QV2 - W
            Else
                '20230816
                Dim KsXXXX As Double = 17.75 * Coeff + 1.825
                Dim KsXXXV As Double = 14.6667 * Coeff + 0.7333
                Dim KsXXX As Double = 11.875 * Coeff + 0.1125
                Dim KsXXV As Double = 7.875 * Coeff - 0.0875
                Dim PhiKs(,) As Double = {{25 / 180 * PI, KsXXV}, {30 / 180 * PI, KsXXX}, {35 / 180 * PI, KsXXXV}, {40 / 180 * PI, KsXXXX}}
                Ks = GetCoeff(TheAverageSoil.Phi, PhiKs, 4)
                TempDes &= TempDesClay.Replace("Qclay", "Qv,b") & Chr(13)
                TempDes &= If(QvName = "", "Qv", QvName) & "=Qv,b-AHγ'+2AH(Hγ'+2p'0)Kstan(φ'/B)" & If(IsBackFlow, "-γ'[A(D-Hc)-Al(D-Hc-" & If(SpudcanParameter.H4 = 0, "Ht", "(H2+H3)") & ")-V]" & Chr(13), Chr(13)) 'Aγ'I=γ’[As(D-Hc)-Al(D-Hc-Ht)V]20240823
                TempDes &= If(QvName = "", "Qv", QvName) & "=" & Round(QV2, 3) & "-" & Round(SpudcanParameter.Area, 3) & "×" & Round(H, 3) & "×" & Round(Weight, 3) & "+" & 2 & "×" & Round(SpudcanParameter.Area, 3) & "×" & Round(H, 3) & "×(" & Round(H, 3) & "×" & Round(Weight, 3) & "+" & 2 & "×" & Round(P0, 3) & ")×" & Round(Ks, 3) & "×tan(" & Round(TheAverageSoil.Phi, 3) & "/" & Round(B, 3) & ")"
                Qv = QV2 - SpudcanParameter.Area * H * Weight + 2 * SpudcanParameter.Area * H * (H * Weight + 2 * P0) * Ks * Tan(TheAverageSoil.Phi / B)
            End If
            If IsBackFlow Then GetBackFlowResult_PunchSand(Qv, Level, LegParameter, SpudcanParameter, Soils, Hc, TempDes)
            TempDes &= "=" & Round(Qv, 3) & ";"
            '取常规破坏模式和穿刺破坏模式的最小值
            If Qv <= Qv_Sand Then
                Description &= TempDes
            Else
                NormalString &= "穿刺破坏结果Qv=Min(常规Qv(" & Qv_Sand & "),穿刺Qv(" & Qv & ")"
                Qv = Qv_Sand
                Description &= TempDesSand
                EsMessageReporter.ReportMessageFunction(NormalString, EsMessageType.Normal)
            End If
        End If

        ''对下部软粘土层进行循环计算求得最小承载力
        'For i As Integer = 0 To Levels.Count - 1
        '    Dim TempDes1 As String = ""
        '    Dim BottomSoil As SoilParameter = GetSoil(Levels(i), Soils)
        '    H = Level - Levels(i)
        '    Dim QV2 As Double
        '    If BottomSoil.SoilType = SoilType.Clay Or BottomSoil.SoilType = SoilType.Both Then
        '        QV2 = GetQV_Clay(Levels(i), LegParameter, SpudcanParameter, BottomSoil, Soils, False, 0, TempDes1, "Qv,b")
        '        Dim Coeff As Double = QV2 / QV1
        '        Dim XL1 As Double = (12 - 3.6) / (0.57 - 0.1)
        '        Dim XL2 As Double = (12 - 2.3) / (0.76 - 0.1)
        '        Dim XL3 As Double = (10.8 - 1.4) / (0.9 - 0.1)
        '        Dim XL4 As Double = (7 - 0.8) / (0.9 - 0.1)
        '        Dim KsCoeffs(,) As Double = {{25 / 180 * PI, 0.8 + (Coeff - 0.1) * XL4}, {30 / 180 * PI, 1.4 + (Coeff - 0.1) * XL3}, {35 / 180 * PI, 2.3 + (Coeff - 0.1) * XL2}, {40 / 180 * PI, 3.6 + (Coeff - 0.1) * XL1}}
        '        Ks = GetCoeff(Soil.Phi, KsCoeffs, 4)
        '        Dim AverageSoil As SoilParameter = GetAverageSoilValue(Soils, Levels(i), Level)
        '        Dim Weight As Double = AverageSoil.Weight
        '        Qv = QV2 - SpudcanParameter.Area * H * Weight + 2 * SpudcanParameter.Area * H * (H * Weight + 2 * P0) * Ks * Tan(Soil.Phi / B)
        '        TempDes1 = TempDes1.Remove(TempDes1.Length - 1, 1)
        '        TempDes1 &= If(QvName = "", "Qv", QvName) & "=Qv,b-AHγ'+2AH(Hγ'+2p'0)Kstan(φ'/B)" & If(IsBackFlow, "-AIγ'" & Chr(13), Chr(13))
        '        TempDes1 &= If(QvName = "", "Qv", QvName) & "=" & Round(QV2, 3) & "-" & Round(SpudcanParameter.Area, 3) & "×" & H & "×" & Round(Weight, 3) & "+" & 2 & "×" & Round(SpudcanParameter.Area, 3) & "×" & H & "×(" & H & "×" & Round(Weight, 3) & "+" & 2 & "×" & Round(P0, 3) & ")×" & Round(Ks, 3) & "×tan(" & Round(Soil.Phi, 3) & "/" & Round(B, 3) & ")"
        '        If IsBackFlow Then GetBackFlowResult_PunchSand(Qv, Level, SpudcanParameter, Soils, Hc, TempDes1)
        '        TempDes1 &= "=" & Round(Qv, 3) & ";"
        '    Else
        '        'QV2 = GetQV_Sand(Levels(i), LegParameter, SpudcanParameter, BottomSoil, Soils)
        '        Qv = 10 ^ 10
        '        TempDes1 = If(QvName = "", "Qv", QvName) & "未计算;"
        '    End If
        '    If i = 0 Then
        '        MinQv = Qv
        '        TempDes = TempDes1
        '    Else
        '        MinQv = Min(MinQv, Qv)
        '        If MinQv = Qv Then TempDes = TempDes1
        '    End If
        'Next
        Return Qv 'MinQv
    End Function
    Function GetQb_Clay(ByVal DeepType As Integer, ByVal CalculateParameter As CalculateParameter, ByVal Level As Double, ByVal LegParameter As LegParameter, ByVal SpudcanParameter As SpudcanParameter, ByVal Soil As SoilParameter, ByVal Soils As List(Of SoilParameter), ByVal fb As Double, Optional ByRef Description As String = "", Optional QbName As String = "") As Double()
        Dim Qb(2) As Double
        Dim HColumn As Double = Max(Soils(0).TopLevel - Level - CalculateParameter.Hc - SpudcanParameter.H3, 0) '桩靴最大截面处上部至海床面距离
        Dim AverageSoil As SoilParameter = GetAverageSoilValue(Soils, Level, Soils(0).TopLevel)
        Dim UpSoil As SoilParameter = GetSoil(Level + 0.001, Soils) '持力土层的上一层土
        For i = 0 To 2
            Dim SuHcol As Double = GetAverageSoilValue(Soils, Level + SpudcanParameter.H3, Soils(0).TopLevel - CalculateParameter.Hc).Su0
            Dim DownSu As Double = (UpSoil.GetSu(Level) + Soil.GetSu(Level)) / 2 'If(HalfDownSu, UpSoil.GetSu(Level) * 0.5, UpDownSu) 'Soil.GetSu(Level)
            Dim SuHt As Double = GetAverageSoilValue(Soils, Level, Level + SpudcanParameter.H3).Su0
            Dim SuHLeg As Double = GetAverageSoilValue(Soils, Level + SpudcanParameter.H3 + SpudcanParameter.H4, Soils(0).TopLevel).Su0
            If i = 2 Then
                SuHcol *= fb
                DownSu *= fb
                SuHt *= fb
                SuHLeg *= fb
            Else
                SuHcol *= i
                DownSu *= i
                SuHt *= i
                SuHLeg *= i
            End If
            Dim Vtop As Double = LegParameter.Area * (HColumn - SpudcanParameter.H4) '桩腿入土体积
            Qb(i) = SpudcanParameter.Weight + SpudcanParameter.Area * (CalculateParameter.NBreakout * DownSu * CalculateParameter.fbase + HColumn * AverageSoil.Weight) - Vtop * AverageSoil.Weight
            If DeepType = 1 Then 'Qbreakout
                '浅埋
                Qb(i) += SpudcanParameter.Circumference * (HColumn * SuHcol * CalculateParameter.ftop + CalculateParameter.alpha * SpudcanParameter.Ht * SuHt * CalculateParameter.fbase)
                Description &= If(QbName = "", "Qu", If(i = 2, QbName, "Qu_C" & i)) & "=W+C(HcolumnSuftop+αHtSufbase)+A(NbreakoutSufbase+Hcolumnγ')-Vtopγ'" & Chr(13)
                Description &= If(QbName = "", "Qu", If(i = 2, QbName, "Qu_C" & i)) & "=" & SpudcanParameter.Weight & "+" & Round(SpudcanParameter.Circumference, 3) & "×(" & Round(HColumn, 3) & "×" & Round(SuHcol, 3) & "×" & CalculateParameter.ftop & "+" & CalculateParameter.alpha & "×" & SpudcanParameter.Ht & "×" & Round(SuHt, 3) & "×" & CalculateParameter.fbase & ")+"
            Else
                '深埋
                Dim HLeg As Double = Max(HColumn - SpudcanParameter.H4, 0)
                Qb(i) += CalculateParameter.fleg * LegParameter.Circumference * HLeg * SuHLeg
                Description &= If(QbName = "", "Qu", If(i = 2, QbName, "Qu_C" & i)) & "=W+flegA'Su+A(NbreakoutSufbase+Hcolumnγ')-Vtopγ'" & Chr(13)
                Description &= If(QbName = "", "Qu", If(i = 2, QbName, "Qu_C" & i)) & "=" & SpudcanParameter.Weight & "+" & CalculateParameter.fleg & "×" & Round(LegParameter.Circumference * HLeg, 3) & "×" & Round(SuHLeg, 3) & "+"
            End If
            Description &= Round(SpudcanParameter.Area, 3) & "×(" & CalculateParameter.NBreakout & "×" & Round(DownSu, 3) & "×" & CalculateParameter.fbase & "+" & Round(HColumn, 3) & "×" & Round(AverageSoil.Weight, 3) & ")-" & Round(Vtop, 3) & "×" & Round(AverageSoil.Weight, 3)
            Description &= "=" & Round(Qb(i), 3) & ";"
        Next
        Return Qb
    End Function
    Function GetQb_Clay_Shallow(ByVal CalculateParameter As CalculateParameter, ByVal Level As Double, ByVal LegParameter As LegParameter, ByVal SpudcanParameter As SpudcanParameter, ByVal Soil As SoilParameter, ByVal Soils As List(Of SoilParameter), Optional HalfDownSu As Boolean = False) As Double '按照浅埋模式计算抗拔力
        Dim Qb As Double
        Dim SFR As Double = GetSideFrictionalResistance(CalculateParameter, Level, LegParameter, SpudcanParameter, Soil, Soils)
        Dim HColumn As Double = Max(Soils(0).TopLevel - Level - CalculateParameter.Hc - SpudcanParameter.H3, 0)
        'Dim SuHcol As Double = GetAverageSoilValue(Soils, Level + SpudcanParameter.H3, Soils(0).TopLevel).Su0
        'Dim SuHt As Double = GetAverageSoilValue(Soils, Level, Level + SpudcanParameter.H3).Su0
        Dim AverageSoil As SoilParameter = GetAverageSoilValue(Soils, Level, Soils(0).TopLevel)
        Dim UpSoil As SoilParameter = GetSoil(Level + 0.001, Soils)
        Dim UpDownSu As Double = GetAverageSoilValue(Soils, Soil.BottomLevel, UpSoil.TopLevel).Su0
        Dim DownSu As Double = If(HalfDownSu, UpSoil.GetSu(Level) * 0.5, UpDownSu) 'Soil.GetSu(Level)
        Dim Vtop As Double = LegParameter.Area * (HColumn - SpudcanParameter.H4)
        'Return SpudcanParameter.Weight + SpudcanParameter.Circumference * (HColumn * SuHcol * CalculateParameter.ftop + CalculateParameter.alpha * SpudcanParameter.Ht * SuHt * CalculateParameter.fbase) + SpudcanParameter.Area * (CalculateParameter.NBreakout * DownSu * CalculateParameter.fbase + HColumn * AverageSoil.Weight) - Vtop * AverageSoil.Weight
        Qb = SpudcanParameter.Weight + SFR + SpudcanParameter.Area * (CalculateParameter.NBreakout * DownSu * CalculateParameter.fbase + HColumn * AverageSoil.Weight) - Vtop * AverageSoil.Weight
        Return Qb
    End Function
    Function GetQb_Clay_Deep(ByVal CalculateParameter As CalculateParameter, ByVal Level As Double, ByVal LegParameter As LegParameter, ByVal SpudcanParameter As SpudcanParameter, ByVal Soil As SoilParameter, ByVal Soils As List(Of SoilParameter), Optional HalfDownSu As Boolean = False) As Double '按照深埋模式计算抗拔力
        Dim Qb As Double
        Dim SFR As Double = GetSideFrictionalResistance(CalculateParameter, Level, LegParameter, SpudcanParameter, Soil, Soils)
        Dim HColumn As Double = Max(Soils(0).TopLevel - Level - CalculateParameter.Hc - SpudcanParameter.H3, 0)
        Dim AverageSoil As SoilParameter = GetAverageSoilValue(Soils, Level, Soils(0).TopLevel)
        'Dim SuHcol As Double = GetAverageSoilValue(Soils, Level + SpudcanParameter.H3, Soils(0).TopLevel).Su0
        Dim UpSoil As SoilParameter = GetSoil(Level + 0.001, Soils)
        Dim UpDownSu As Double = GetAverageSoilValue(Soils, Soil.BottomLevel, UpSoil.TopLevel).Su0
        Dim DownSu As Double = If(HalfDownSu, UpSoil.GetSu(Level) * 0.5, UpDownSu) 'Soil.GetSu(Level)
        Dim Vtop As Double = LegParameter.Area * (HColumn - SpudcanParameter.H4)
        'Return SpudcanParameter.Weight + LegParameter.Circumference * SuHcol * (HColumn - SpudcanParameter.H4) + SpudcanParameter.Area * (CalculateParameter.NBreakout * DownSu * CalculateParameter.fbase + HColumn * AverageSoil.Weight) - Vtop * AverageSoil.Weight
        Qb = SpudcanParameter.Weight + SFR + SpudcanParameter.Area * (CalculateParameter.NBreakout * DownSu * CalculateParameter.fbase + HColumn * AverageSoil.Weight) - Vtop * AverageSoil.Weight
        Return Qb
    End Function
    Function GetSideFrictionalResistance(ByVal CalculateParameter As CalculateParameter, ByVal Level As Double, ByVal LegParameter As LegParameter, ByVal SpudcanParameter As SpudcanParameter, ByVal Soil As SoilParameter, ByVal Soils As List(Of SoilParameter))
        '获得插深范围内土层侧摩阻力（每层土按土类型进行计算，计算时埋深类型按插深考虑）
        Dim SFR As Double
        Dim SFRClay_Shallow, SFRClay_Deep, SFRSand As Double
        Dim D As Double = Soils(0).TopLevel - Level
        Dim HColumnLevel As Double = Level + SpudcanParameter.H3
        Dim LegLevel As Double = HColumnLevel + SpudcanParameter.H4
        Dim HcLevel As Double = Soils(0).TopLevel - CalculateParameter.Hc
        Dim SpudcanB As Double
        SpudcanB = If(CalculateParameter.IsEquivalentToCircleSpudcan, 2 * (SpudcanParameter.Area / PI) ^ 0.5, If(SpudcanParameter.ShapeType = 0, SpudcanParameter.Diameter, Min(SpudcanParameter.L, SpudcanB))) '20230804
        If D <= SpudcanB Then
            Dim HColumn_Clay As Double
            Dim SuHcol As Double = GetAverageSoilValue(Soils, HColumnLevel, HcLevel, HColumn_Clay, SoilType.Clay).Su0
            Dim Ht_Clay As Double
            Dim SuHt As Double = GetAverageSoilValue(Soils, Level, HColumnLevel, Ht_Clay, SoilType.Clay).Su0
            SFRClay_Shallow += SpudcanParameter.Circumference * (SuHcol * HColumn_Clay * CalculateParameter.ftop + CalculateParameter.alpha * SuHt * Ht_Clay * CalculateParameter.fbase)
        Else
            Dim HLeg_Clay As Double
            Dim SuHLeg As Double = GetAverageSoilValue(Soils, LegLevel, HcLevel, HLeg_Clay, SoilType.Clay).Su0
            SFRClay_Deep += LegParameter.Circumference * SuHLeg * HLeg_Clay
        End If

        Dim H As Double = GetH(Soil.Phi, SpudcanB)
        Dim S As Double = GetS(Soil.Phi)
        Dim SumH As Double
        For Each ASoil In Soils
            If ASoil.BottomLevel < HcLevel And ASoil.TopLevel > Level Then
                If ASoil.SoilType = SoilType.Both Or ASoil.SoilType = SoilType.Sand Then
                    'D_Sand：插深D内该土层的砂土高度
                    Dim D_Sand As Double = Min(ASoil.TopLevel, HcLevel) - Max(ASoil.BottomLevel, Level)
                    Dim c As Double = ASoil.GetSu(Max(ASoil.BottomLevel, Level)) '抗剪强度su和粘结力c物理含义相同* *
                    SumH += D_Sand
                    SFRSand += 2 * c * D_Sand * (SpudcanB + SpudcanParameter.L)
                    Dim Ku As Double = GetKu(ASoil.Phi)
                    If H < D Then
                        Dim Phi_H, H_Sand As Double 'H_Sand：插深D~（D-H）内该土层的砂土高度
                        If ASoil.BottomLevel < Level + H And ASoil.TopLevel > Level Then
                            H_Sand = Min(ASoil.TopLevel, Level + H) - Max(ASoil.BottomLevel, Level)
                            Phi_H = ASoil.Phi
                        End If
                        SFRSand += ASoil.Weight * Ku * (2 * D_Sand * H_Sand * Tan(ASoil.Phi) - H_Sand * H_Sand * Tan(Phi_H)) * (2 * S * SpudcanB + SpudcanParameter.L - SpudcanB)
                    Else
                        SFRSand += ASoil.Weight * Ku * D_Sand ^ 2 * Tan(ASoil.Phi) * (2 * S * SpudcanB + SpudcanParameter.L - SpudcanB)
                    End If
                End If
            End If
        Next
        'Dim c, D_Sand As Double 'D_Sand：插深D内砂土总高度
        'c = GetAverageSoilValue(Soils, Level, HcLevel, D_Sand, SoilType.Sand).C
        'SFRSand += 2 * c * D_Sand * (SpudcanB + SpudcanParameter.L)
        'Dim Phi_D, Phi_H, H_Sand As Double 'H_Sand：插深D~（D-H）内砂土总高度
        'Phi_D = GetAverageSoilValue(Soils, Level, HcLevel, D_Sand, SoilType.Sand).Phi 'D_Sand深度内的平均摩擦角
        'Phi_H = GetAverageSoilValue(Soils, Level, Level + H, H_Sand, SoilType.Sand).Phi 'H_Sand深度内的平均摩擦角
        'Dim AverageSoil As SoilParameter = GetAverageSoilValue(Soils, Level, Soils(0).TopLevel)
        ''If(H < D, (2 * D - H) * H, D ^ 2)
        'SFRSand += If(H < D, (2 * D_Sand * H_Sand * Tan(Phi_D) - H_Sand * H_Sand * Tan(Phi_H)), D_Sand ^ 2 * Tan(Phi_D)) * AverageSoil.Weight * (2 * S * SpudcanB + SpudcanParameter.L - SpudcanB) * Ku
        SFR = SFRClay_Shallow + SFRClay_Deep + SFRSand
        Return SFR
    End Function
    Function GetQb_Sand(ByVal DeepType As Integer, ByVal CalculateParameter As CalculateParameter, ByVal Level As Double, ByVal LegParameter As LegParameter, ByVal SpudcanParameter As SpudcanParameter, ByVal Soil As SoilParameter, ByVal Soils As List(Of SoilParameter), ByVal fb As Double, ByVal H As Double, Optional ByRef Description As String = "", Optional QbName As String = "") As Double()
        Dim Qb(2) As Double
        Dim D As Double = Soils(0).TopLevel - Level
        Dim AverageSoil As SoilParameter = GetAverageSoilValue(Soils, Level, Soils(0).TopLevel)
        For i = 0 To 2
            'Dim H As Double = GetH(AverageSoil.Phi, SpudcanParameter.B) '判别深度H，插深D处往上高度，不考虑fb20230620
            Dim S As Double = GetS(AverageSoil.Phi) '最大形状系数S
            Dim SoilPhi, AverageSoilPhi As Double
            Dim AverageSoilc As Double '抗剪强度su和粘结力c物理含义相同* *
            If i = 2 Then
                SoilPhi = fb * Soil.Phi
                AverageSoilPhi = fb * AverageSoil.Phi
                AverageSoilc = fb * AverageSoil.Su0
            Else
                SoilPhi = i * Soil.Phi
                AverageSoilPhi = i * AverageSoil.Phi
                AverageSoilc = i * AverageSoil.Su0
            End If
            Dim Ku As Double = GetKu(SoilPhi)
            Dim HColumn As Double = Max(Soils(0).TopLevel - Level - CalculateParameter.Hc - SpudcanParameter.H3, 0) '桩靴最大截面处上部至海床面距离
            Dim Vtop As Double = LegParameter.Area * (HColumn - SpudcanParameter.H4) '桩腿入土体积
            Qb(i) = SpudcanParameter.Weight + SpudcanParameter.Area * HColumn * AverageSoil.Weight - Vtop * AverageSoil.Weight + 2 * AverageSoilc * D * (SpudcanParameter.B + SpudcanParameter.L)
            Qb(i) += AverageSoil.Weight * (2 * S * SpudcanParameter.B + SpudcanParameter.L - SpudcanParameter.B) * Ku * Tan(AverageSoilPhi) * If(DeepType = 1, D ^ 2, (2 * D - H) * H)
            Description &= If(QbName = "", "Qu", If(i = 2, "Qu_Sand", "Qu_S" & i)) & "=2cD(B+L)+γ" & If(DeepType = 1, "D^2", "(2D-H)H") & "(2sB+L-B)Kutanφ+W+AHcolumnγ'-Vtopγ'" & Chr(13)
            Description &= If(QbName = "", "Qu", If(i = 2, "Qu_Sand", "Qu_S" & i)) & "=2×" & Round(AverageSoilc, 3) & "×" & D & "×(" & Round(SpudcanParameter.B, 3) & "+" & SpudcanParameter.L & ")+" & Round(AverageSoil.Weight, 3) & "×" & If(DeepType = 1, D & "^2", "(2×" & D & "-" & H & ")×" & H)
            Description &= "×(2×" & Round(S, 3) & "×" & Round(SpudcanParameter.B, 3) & "+" & SpudcanParameter.L & "-" & Round(SpudcanParameter.B, 3) & ")×" & Ku & "×Tan(" & Round(AverageSoilPhi, 3) & ")+" & SpudcanParameter.Weight & "+" & Round(SpudcanParameter.Area, 3) & "×" & Round(HColumn, 3) & "×" & Round(AverageSoil.Weight, 3) & "-" & Round(Vtop, 3) & "×" & Round(AverageSoil.Weight, 3)
            Description &= "=" & Round(Qb(i), 3) & ";"
        Next
        Return Qb
    End Function
    Function GetQb_Sand(ByVal CalculateParameter As CalculateParameter, ByVal Level As Double, ByVal LegParameter As LegParameter, ByVal SpudcanParameter As SpudcanParameter, ByVal Soil As SoilParameter, ByVal Soils As List(Of SoilParameter)) As Double
        Dim Qb As Double
        Qb = SpudcanParameter.Weight + GetSideFrictionalResistance(CalculateParameter, Level, LegParameter, SpudcanParameter, Soil, Soils)
        Return Qb
    End Function
    Function GetKu(SoilPhi As Double) As Double
        Dim KuPhi(,) As Double = {{4 / 180 * PI, 0.7}, {6 / 180 * PI, 0.72}, {8 / 180 * PI, 0.74}, {10 / 180 * PI, 0.76}, {12 / 180 * PI, 0.78}, {14 / 180 * PI, 0.79}, {16 / 180 * PI, 0.82}, {18 / 180 * PI, 0.83}, {20 / 180 * PI, 0.85}, {22 / 180 * PI, 0.87}, {24 / 180 * PI, 0.88}, {26 / 180 * PI, 0.89}, {28 / 180 * PI, 0.9}, {30 / 180 * PI, 0.92}, {32 / 180 * PI, 0.93}, {34 / 180 * PI, 0.94}, {36 / 180 * PI, 0.946}, {38 / 180 * PI, 0.953}, {40 / 180 * PI, 0.958}, {42 / 180 * PI, 0.961}, {44 / 180 * PI, 0.962}, {45 / 180 * PI, 0.962}}
        Dim Ku As Double = GetCoeff(SoilPhi, KuPhi, 22)
        Return Ku
    End Function
End Class




