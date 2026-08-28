Imports System.Data
Imports System.Math
Imports Easy, Easy.EasyTool
Imports Easy.Structure.Soil
Public Class SpudcanDB
    Private MydataSet As DataSet
    Public MonitorScale As Double = 1.0
    Private ABoatBB As String = "1020" '多船模式下单船更新的版本号
    Private BB As String = "10004" '单船或多船模式下多船更新的版本号
    Private Shared ReadOnly SelectTabNames As String() = {"LS_Common", "LS_LegType", "LS_SpudcanType", "LS_SoilType", "LS_ExcelDrillingName", "LS_TempSoilDrilling", "LS_SoilDrillingParameter", "LS_Boat", "LS_StructureData", "LS_DeepType", "LS_TempDeepType1", "LS_TempDeepType2", "LS_ComputingModelType_Qv", "LS_ComputingModelType_Qb"}
    Sub New(ADataSet As DataSet, CreateTable As Boolean, Optional ByVal Boats As Boolean = True)
        Me.MydataSet = ADataSet
        If CreateTable Then
            CreateDatabase(ADataSet, Boats)
        End If
    End Sub
    'Sub OpenFile(ByVal FileName As String)
    '    StructureKit.OpenFile(FileName)
    '    MydataSet = StructureKit.StructureData.GetData
    '    UpdateData(MydataSet.Tables.Contains("LS_Boat"))
    'End Sub
    Shared Function GetNotResultTabNames() As String()
        Return SelectTabNames.Concat({"LS_CalculationParameter", "LS_StructureData"}).ToArray
    End Function
    Public Sub CreateDatabase(ByRef ADataSet As DataSet, Optional ByVal Boats As Boolean = True) 'Boats=0时获得单艘船和多艘船数据库，Boats=1时获得多艘船数据库
        MydataSet = ADataSet
        CreateCommonTable(Boats)
        CreateSoilTable(Boats) '土参数
        CreateComponentTable(Boats)
        CreateStructureParameterTable()
        CreateMeshTable()
        CreateResultTable()
        If Boats Then
            CreateBoatDatabaseByABoat()
        End If

        ADataSet.AcceptChanges()
    End Sub
    Sub CreateBoatDatabaseByABoat()
        'Dim MyDataSet As DataSet = Application.GetStructureKit.GetData
        For Each ATable As DataTable In MydataSet.Tables
            If ATable.TableName.Contains("LS_") Then
                If Not SelectTabNames.Contains(ATable.TableName) Then
                    ATable.Columns.Add("BoatID", System.Type.GetType("System.Int32"))
                    ATable.Columns("BoatID").DefaultValue = 1
                    For Each row In ATable.Rows
                        row("BoatID") = 1
                    Next
                    If ATable.TableName = "LS_CalculationParameter" Then
                        ATable.Columns.Remove("PressForce")
                    End If
                Else
                    If ATable.TableName = "LS_StructureData" Then
                        ATable.Columns.Remove("BoatName")
                        ATable.Columns.Remove("PullingCapacity")
                        ATable.Columns.Remove("AirGap")
                    End If
                End If
            End If
        Next
    End Sub
    Sub GetBoatsDataFromABoat()
        If Not MydataSet.Tables.Contains("LS_Boat") Then
            Dim NewRow As DataRow
            CreateBoatSoilTable()
            MydataSet.Tables("LS_SoilDrillingParameter").Clear()
            MydataSet.Tables("LS_TempSoilDrilling").Clear()
            If MydataSet.Tables("LS_Common").Rows(0)("UseSingleDrilling") Then
                NewRow = MydataSet.Tables("LS_TempSoilDrilling").Rows.Add
                NewRow("BoatID") = 1
                NewRow("DrillingID") = 1
                NewRow("DrillingName") = "钻孔#1"
                For Each row In MydataSet.Tables("LS_LegSoilLayer").Rows
                    NewRow = MydataSet.Tables("LS_SoilDrillingParameter").Rows.Add
                    NewRow("BoatID") = 1
                    NewRow("DrillingID") = 1
                    NewRow("DrillingName") = "钻孔#1"
                    NewRow("ID") = row("SoilID")
                    NewRow("TopLevel") = row("TopLevel")
                    Dim SoilRow As DataRow = MydataSet.Tables("LS_Soil").Select("ID=" & row("SoilID"))(0)
                    NewRow("Name") = SoilRow("Name")
                    NewRow("Type") = SoilRow("Type")
                    If SoilRow("DSu") = 0 Then NewRow("Su") = SoilRow("Su0")
                    NewRow("UnderWaterPhi") = SoilRow("UnderWaterPhi")
                    NewRow("UnderWaterWeight") = SoilRow("UnderWaterWeight")
                Next
                For Each SoilRow In MydataSet.Tables("LS_Soil").Select("DSu<>0")
                    Dim Rows As DataRow() = MydataSet.Tables("LS_LegSoilLayer").Select("SoilID=" & SoilRow("ID"), "TopLevel DESC")
                    If Rows.Count = 1 Then
                        Rows(0)("Su") = SoilRow("Su0")
                    Else
                        For Each ARow In Rows
                            ARow("Su") = SoilRow("Su0") + SoilRow("DSu") * (Rows(0)("TopLevel") - SoilRow("TopLevel"))
                        Next
                    End If
                Next
            Else
                For Each row In MydataSet.Tables("LS_SoilDrilling").Rows
                    NewRow = MydataSet.Tables("LS_TempSoilDrilling").Rows.Add
                    NewRow("BoatID") = 1
                    NewRow("DrillingID") = row("ID")
                    NewRow("DrillingName") = row("Name")
                    For i = 0 To row("SoilLayers").ToString.Split(";").Length - 1
                        NewRow = MydataSet.Tables("LS_SoilDrillingParameter").Rows.Add
                        NewRow("BoatID") = 1
                        NewRow("DrillingID") = row("ID")
                        NewRow("DrillingName") = row("Name")
                        NewRow("Name") = row("SoilLayers").ToString.Split(";")(i).Split(",").First
                        NewRow("TopLevel") = row("SoilLayers").ToString.Split(";")(i).Split(",").Last
                        Dim SoilRow As DataRow = MydataSet.Tables("LS_Soil").Select("Name='" & NewRow("Name") & "'")(0)
                        NewRow("ID") = SoilRow("ID") '仅显示，无其他作用
                        NewRow("Type") = SoilRow("Type")
                        If SoilRow("DSu") = 0 Then NewRow("Su") = SoilRow("Su0")
                        NewRow("UnderWaterPhi") = SoilRow("UnderWaterPhi")
                        NewRow("UnderWaterWeight") = SoilRow("UnderWaterWeight")
                    Next
                Next
                For Each row In MydataSet.Tables("LS_SoilDrilling").Rows
                    For Each SoilRow In MydataSet.Tables("LS_Soil").Select("DSu<>0")
                        Dim Rows As DataRow() = MydataSet.Tables("LS_SoilDrillingParameter").Select("Name='" & SoilRow("Name") & "' and DrillingID=" & row("ID"), "TopLevel DESC")
                        If Rows.Count = 1 Then
                            Rows(0)("Su") = SoilRow("Su0")
                        Else
                            For Each ARow In Rows
                                ARow("Su") = SoilRow("Su0") + SoilRow("DSu") * (Rows(0)("TopLevel") - SoilRow("TopLevel"))
                            Next
                        End If
                    Next
                Next
            End If

            CreateBoatTable()
            MydataSet.Tables("LS_Boat").Rows.Clear()
            NewRow = MydataSet.Tables("LS_Boat").Rows.Add
            NewRow("ID") = 1
            NewRow("Name") = MydataSet.Tables("LS_StructureData").Rows(0)("BoatName")
            NewRow("W") = MydataSet.Tables("LS_Spudcan").Rows(0)("Weight") / 9.8
            NewRow("PullingCapacity") = MydataSet.Tables("LS_StructureData").Rows(0)("PullingCapacity")
            NewRow("AirGap") = MydataSet.Tables("LS_StructureData").Rows(0)("AirGap")
            'NewRow("LegPressForce") = MydataSet.Tables("LS_CalculationParameter").Rows(0)("PressForce")
            NewRow("SumW") = MydataSet.Tables("LS_CalculationParameter").Rows(0)("PressForce")
            NewRow("LegType") = MydataSet.Tables("LS_Leg").Rows(0)("Type")
            NewRow("LegDiameter") = MydataSet.Tables("LS_Leg").Rows(0)("Diameter")
            NewRow("LegCircumference") = MydataSet.Tables("LS_Leg").Rows(0)("Circumference")
            NewRow("LegHLN") = MydataSet.Tables("LS_Leg").Rows(0)("ActiveLength")
            NewRow("LegA") = MydataSet.Tables("LS_Leg").Rows(0)("Area")
            NewRow("SpudcanShapeType") = MydataSet.Tables("LS_Spudcan").Rows(0)("ShapeType")
            NewRow("SpudcanL") = If(NewRow("SpudcanShapeType") = 0, "-", MydataSet.Tables("LS_Spudcan").Rows(0)("L"))
            NewRow("SpudcanB") = If(NewRow("SpudcanShapeType") = 0, MydataSet.Tables("LS_Spudcan").Rows(0)("Diameter"), MydataSet.Tables("LS_Spudcan").Rows(0)("B"))
            'NewRow("SpudcanH") = Val(MydataSet.Tables("LS_Spudcan").Rows(0)("Parameter").ToString.Split({"H2="}, StringSplitOptions.RemoveEmptyEntries).Last.Split(",").First)
            NewRow("SpudcanParameter") = MydataSet.Tables("LS_Spudcan").Rows(0)("Parameter")
            NewRow("SpudcanA") = MydataSet.Tables("LS_Spudcan").Rows(0)("Area")
            NewRow("SpudcanCircumference") = MydataSet.Tables("LS_Spudcan").Rows(0)("Circumference")
            NewRow("SpudcanV") = MydataSet.Tables("LS_Spudcan").Rows(0)("Volume")

            Dim ABoatBB As Integer = MydataSet.Tables("LS_Common").Rows(0)("BB")
            MydataSet.Tables.Remove("LS_Common")
            CreateBoatCommonTable()
            MydataSet.Tables("LS_Common").Rows(0)("ABoatBB") = ABoatBB

            Dim RemoveTabNames As String() = {"LS_Soil", "LS_SoilDrilling", "LS_LegSoilLayer", "LS_Leg", "LS_Spudcan"}
            For Each ATabName In RemoveTabNames
                MydataSet.Tables.Remove(ATabName)
            Next
            CreateBoatDatabaseByABoat()
        End If
    End Sub
    Function GetBoatAppDic() As Dictionary(Of Integer, DataSet)
        Dim BoatsDataSet As DataSet = MydataSet
        Dim BoatAppDic As New Dictionary(Of Integer, DataSet)
        Dim ABoatDataSet As DataSet
        For Each Brow In BoatsDataSet.Tables("LS_Boat").Rows
            Dim StructureKit As New DataSet 'EasyStructureKit(System.Windows.Forms.Application.StartupPath)
            'Dim AnApplication = New EsApplication(StructureKit)
            BoatAppDic.Add(Brow("ID"), StructureKit)
            CreateDatabase(StructureKit, False)
            ABoatDataSet = StructureKit

            Dim NewRow As DataRow
            For Each ATable As DataTable In BoatsDataSet.Tables
                If ATable.TableName.Contains("LS_") Then
                    Select Case ATable.TableName
                        Case "LS_Common"
                            ABoatDataSet.Tables(ATable.TableName).Clear()
                            Dim TheTable As DataTable = ABoatDataSet.Tables(ATable.TableName)
                            For Each Trow In ATable.Select("")
                                NewRow = TheTable.Rows.Add
                                NewRow("BB") = Trow("ABoatBB")
                                NewRow("UseSingleDrilling") = False '默认按多钻孔导入数据
                                NewRow("UseSoilDrilling") = True
                                NewRow("Legx0") = 0
                                NewRow("Legy0") = 0
                                NewRow("SoilLayerMeshSize") = 5
                                NewRow("SuInputType") = 1
                            Next
                        Case "LS_SoilDrillingParameter"
                            ABoatDataSet.Tables("LS_Soil").Clear()
                            ABoatDataSet.Tables("LS_SoilDrilling").Clear()
                            For Each Trow In ATable.Select("BoatID=" & Brow("ID"))
                                NewRow = ABoatDataSet.Tables("LS_Soil").Rows.Add
                                NewRow("ID") = Trow("ID")
                                NewRow("Name") = Trow("Name")
                                NewRow("Type") = Trow("Type")
                                NewRow("Su0") = Trow("Su")
                                NewRow("UnderWaterWeight") = Trow("UnderWaterWeight")
                                NewRow("UnderWaterPhi") = Trow("UnderWaterPhi")
                                NewRow("UnderWaterC") = Trow("Su")

                                NewRow("DrillingID") = Trow("DrillingID")

                                If ABoatDataSet.Tables("LS_SoilDrilling").Select("ID=" & Trow("DrillingID")).Count = 0 Then
                                    NewRow = ABoatDataSet.Tables("LS_SoilDrilling").Rows.Add
                                    NewRow("ID") = Trow("DrillingID")
                                    NewRow("Name") = Trow("DrillingName")
                                    NewRow("x") = 0
                                    NewRow("y") = 0
                                    For Each Trow1 In ATable.Select("BoatID=" & Brow("ID") & " and DrillingID=" & Trow("DrillingID"), "TopLevel DESC")
                                        NewRow("SoilLayers") &= Trow1("Name") & "," & Trow1("TopLevel") & ";"
                                    Next
                                    NewRow("SoilLayers") = NewRow("SoilLayers").ToString.Remove(NewRow("SoilLayers").ToString.Length - 1, 1)
                                End If
                            Next
                        Case "LS_Boat"
                            ABoatDataSet.Tables("LS_Leg").Clear()
                            ABoatDataSet.Tables("LS_Spudcan").Clear()
                            NewRow = ABoatDataSet.Tables("LS_Leg").Rows.Add
                            NewRow("ID") = 1
                            NewRow("Type") = Brow("LegType") '默认为圆形，类型，圆形-1,桁架-2
                            NewRow("Circumference") = Brow("LegCircumference")
                            NewRow("Diameter") = Brow("LegDiameter")
                            NewRow("Area") = Brow("LegA")
                            NewRow("Volume0") = 0
                            NewRow("Volume") = 0
                            NewRow("Weight") = 0
                            NewRow("Parameter") = ""
                            NewRow("TopLevel") = 0
                            NewRow("ActiveLength") = Brow("LegHLN")
                            NewRow = ABoatDataSet.Tables("LS_Spudcan").Rows.Add
                            NewRow("ID") = 1
                            NewRow("Type") = 1 '默认为类四边形，已不使用
                            NewRow("ShapeType") = Brow("SpudcanShapeType") '默认为方形，方形-1,圆形-0
                            NewRow("B") = Brow("SpudcanB")
                            NewRow("L") = If(Brow("SpudcanShapeType") = 0, Brow("SpudcanB"), Val(Brow("SpudcanL")))
                            NewRow("Diameter") = Brow("SpudcanB") '界面提示* *
                            NewRow("Circumference") = Brow("SpudcanCircumference")
                            NewRow("Area") = Brow("SpudcanA")
                            NewRow("Volume") = Brow("SpudcanV")
                            NewRow("Weight") = Brow("W") * 9.8
                            NewRow("Parameter") = Brow("SpudcanParameter") '"H2=" & Brow("SpudcanH")
                        Case Else
                            If ABoatDataSet.Tables.Contains(ATable.TableName) Then
                                Dim TheTable As DataTable = ABoatDataSet.Tables(ATable.TableName)
                                TheTable.Clear()
                                For Each Trow In ATable.Select(If(SelectTabNames.Contains(ATable.TableName), "", "BoatID=" & Brow("ID")))
                                    NewRow = TheTable.Rows.Add
                                    If ATable.TableName = "LS_CalculationParameter" Then
                                        NewRow("PressForce") = Brow("SumW") 'Brow("LegPressForce")
                                    End If
                                    If ATable.TableName = "LS_StructureData" Then
                                        NewRow("BoatName") = Brow("Name")
                                        NewRow("PullingCapacity") = Brow("PullingCapacity")
                                        NewRow("AirGap") = Brow("AirGap")
                                    End If
                                    For i = 0 To TheTable.Columns.Count - 1
                                        For j = 0 To ATable.Columns.Count - 1
                                            If TheTable.Columns(i).ColumnName = ATable.Columns(j).ColumnName Then
                                                NewRow(i) = Trow(j)
                                                Exit For
                                            End If
                                        Next
                                    Next
                                Next
                            End If
                    End Select
                End If
            Next
        Next
        Return BoatAppDic
    End Function
    Sub CreateCommonTable(Optional ByVal Boats As Boolean = True)
        Dim ATable As DataTable
        Dim NewRow As DataRow
        'Dim MyDataSet As DataSet = Application.GetStructureKit.GetData
        If Boats Then
            CreateBoatCommonTable()
            Exit Sub
        End If
        ATable = New DataTable("LS_Common")
        MydataSet.Tables.Add(ATable)
        With ATable.Columns
            .Add("BB", System.Type.GetType("System.String"))
            .Add("UseSingleDrilling", System.Type.GetType("System.Boolean")) '单/多孔计算模式
            .Add("UseSoilDrilling", System.Type.GetType("System.Boolean"))
            .Add("Legx0", System.Type.GetType("System.Double"))
            .Add("Legy0", System.Type.GetType("System.Double"))
            .Add("SoilLayerMeshSize", System.Type.GetType("System.Double"))
            .Add("SuInputType", System.Type.GetType("System.Double"))

        End With
        ATable.Columns("BB").DefaultValue = ABoatBB  '设置默认值
        ATable.Columns("UseSingleDrilling").DefaultValue = False '设置默认值
        ATable.Columns("UseSoilDrilling").DefaultValue = True '设置默认值
        ATable.Columns("Legx0").DefaultValue = 0 '设置默认值
        ATable.Columns("Legy0").DefaultValue = 0 '设置默认值
        ATable.Columns("SoilLayerMeshSize").DefaultValue = 5 '设置默认值
        ATable.Columns("SuInputType").DefaultValue = 1 '设置默认值，1-线性输入，2-表格(暂不使用)
        NewRow = ATable.Rows.Add
        NewRow("BB") = ABoatBB
        NewRow("Legx0") = 0
        NewRow("Legy0") = 0
        NewRow("SoilLayerMeshSize") = 5
        NewRow("UseSingleDrilling") = False
        NewRow("UseSoilDrilling") = True
        NewRow("SuInputType") = 1

    End Sub

    Sub CreateBoatCommonTable()
        Dim ATable As DataTable
        Dim NewRow As DataRow
        'Dim MyDataSet As DataSet = Application.GetStructureKit.GetData 
        ATable = New DataTable("LS_Common")
        MydataSet.Tables.Add(ATable)
        With ATable.Columns
            .Add("BB", System.Type.GetType("System.String"))
            .Add("ABoatBB", System.Type.GetType("System.String"))
        End With
        ATable.Columns("BB").DefaultValue = BB '设置默认值
        ATable.Columns("ABoatBB").DefaultValue = ABoatBB '设置默认值
        NewRow = ATable.Rows.Add
    End Sub
    Sub CreateBoatSoilTable()
        Dim ATable As DataTable
        Dim NewRow As DataRow
        'Dim MyDataSet As DataSet = Application.GetStructureKit.GetData
        ATable = New DataTable("LS_ExcelDrillingName")
        MydataSet.Tables.Add(ATable)
        With ATable.Columns
            .Add("BoatID", System.Type.GetType("System.Int32"))
            .Add("SheetName", System.Type.GetType("System.String"))
            .Add("DrillingName", System.Type.GetType("System.String"))
        End With
        ATable.Columns("BoatID").DefaultValue = 1 '设置默认值
        ATable.Columns("SheetName").DefaultValue = "" '设置默认值
        ATable.Columns("DrillingName").DefaultValue = "" '设置默认值

        ATable = New DataTable("LS_TempSoilDrilling")
        MydataSet.Tables.Add(ATable)
        With ATable.Columns
            .Add("BoatID", System.Type.GetType("System.Int32"))
            .Add("DrillingID", System.Type.GetType("System.Int32"))
            .Add("DrillingName", System.Type.GetType("System.String"))
        End With
        ATable.Columns("BoatID").DefaultValue = 1 '设置默认值
        ATable.Columns("DrillingID").DefaultValue = 1 '设置默认值
        ATable.Columns("DrillingName").DefaultValue = "" '设置默认值

        ATable = New DataTable("LS_SoilDrillingParameter")
        MydataSet.Tables.Add(ATable)
        With ATable.Columns
            .Add("BoatID", System.Type.GetType("System.Int32"))
            .Add("DrillingID", System.Type.GetType("System.Int32"))
            .Add("DrillingName", System.Type.GetType("System.String"))
            .Add("ID", System.Type.GetType("System.Int32")) '土层ID
            .Add("Name", System.Type.GetType("System.String")) '土层名称-1:①2淤泥质土
            .Add("TopLevel", System.Type.GetType("System.Double")) '层顶标高
            .Add("TipLevel", System.Type.GetType("System.Double")) '层底标高
            .Add("Type", System.Type.GetType("System.Int32")) '0-粘土，1-砂土，2-不确定
            .Add("Su", System.Type.GetType("System.Double")) '不排水抗剪强度(水下粘结力)
            .Add("UnderWaterPhi", System.Type.GetType("System.Double")) '水下摩擦角
            .Add("UnderWaterWeight", System.Type.GetType("System.Double")) '水下重度，浮重度* *
            .Add("N", System.Type.GetType("System.Double")) '标贯击数
        End With
        ATable.Columns("BoatID").DefaultValue = 1 '设置默认值
        ATable.Columns("DrillingID").DefaultValue = 1 '设置默认值
        ATable.Columns("DrillingName").DefaultValue = "钻孔1" '设置默认值 
        ATable.Columns("ID").DefaultValue = 1 '设置默认值
        ATable.Columns("Name").DefaultValue = "1:①2淤泥质土" '设置默认值
        ATable.Columns("TopLevel").DefaultValue = 0 '设置默认值
        ATable.Columns("TipLevel").DefaultValue = 0 '设置默认值
        ATable.Columns("Type").DefaultValue = 0 '设置默认值
        ATable.Columns("Su").DefaultValue = 10 '设置默认值
        ATable.Columns("UnderWaterPhi").DefaultValue = 0 '设置默认值
        ATable.Columns("UnderWaterWeight").DefaultValue = 8 '设置默认值
        ATable.Columns("N").DefaultValue = 0 '设置默认值

        NewRow = ATable.Rows.Add
        NewRow("TopLevel") = 2
    End Sub
    Sub CreateSoilTable(Optional ByVal Boats As Boolean = True)
        Dim ATable As DataTable
        Dim NewRow As DataRow
        'Dim MyDataSet As DataSet = Application.GetStructureKit.GetData
        ATable = New DataTable("LS_SoilType")
        MydataSet.Tables.Add(ATable)
        With ATable.Columns
            .Add("ID", System.Type.GetType("System.Int32")) '土层ID 
            .Add("Name", System.Type.GetType("System.String")) '0-黏土，1-砂土，2-不确定 
        End With
        NewRow = ATable.Rows.Add
        NewRow("ID") = 0
        NewRow("Name") = "黏土"
        NewRow = ATable.Rows.Add
        NewRow("ID") = 1
        NewRow("Name") = "砂土"
        NewRow = ATable.Rows.Add
        NewRow("ID") = 2
        NewRow("Name") = "复合土"
        If Boats Then
            CreateBoatSoilTable()
            Exit Sub
        End If
        ATable = New DataTable("LS_Soil")
        MydataSet.Tables.Add(ATable)
        With ATable.Columns
            .Add("DrillingID", System.Type.GetType("System.Int32")) '钻孔号，仅在多船版本计算时将多船数据导入单船数据时使用* *
            .Add("ID", System.Type.GetType("System.Int32")) '土层ID
            .Add("Name", System.Type.GetType("System.String")) '名称
            .Add("Type", System.Type.GetType("System.Int32")) '0-粘土，1-砂土，2-不确定
            .Add("Su", System.Type.GetType("System.String")) '不排水抗剪强度//表格输入
            .Add("Su0", System.Type.GetType("System.Double")) '水下粘结力
            .Add("DSu", System.Type.GetType("System.Double")) '强度增长系数
            .Add("UnderWaterWeight", System.Type.GetType("System.Double")) '水下重度，浮重度* *
            .Add("UnderWaterPhi", System.Type.GetType("System.Double")) '水下摩擦角
            .Add("UnderWaterC", System.Type.GetType("System.Double")) '水下粘结力，废除且合并为不排水抗剪强度* *
            .Add("E", System.Type.GetType("System.Double")) '弹性模量
            .Add("mu", System.Type.GetType("System.Double")) '泊松比

            .Add("OnLegWeightReduceCoeff", System.Type.GetType("System.Double")) '折减系数
            .Add("OnLegStrenthengReduceCoeff", System.Type.GetType("System.Double")) '折减系数
            .Add("OnLegEReduceCoeff", System.Type.GetType("System.Double")) '折减系数
            .Add("OnLegMuReduceCoeff", System.Type.GetType("System.Double")) '折减系数


        End With
        ATable.Columns("DrillingID").DefaultValue = 1
        ATable.Columns("ID").DefaultValue = 1 '设置默认值
        ATable.Columns("Name").DefaultValue = 1 '设置默认值
        ATable.Columns("Type").DefaultValue = 0 '设置默认值
        ATable.Columns("Su").DefaultValue = "" '设置默认值
        ATable.Columns("Su0").DefaultValue = 0 '设置默认值
        ATable.Columns("DSu").DefaultValue = 0 '设置默认值
        ATable.Columns("UnderWaterWeight").DefaultValue = 8 '设置默认值
        ATable.Columns("UnderWaterPhi").DefaultValue = 0 '设置默认值
        ATable.Columns("UnderWaterC").DefaultValue = 0 '设置默认值
        ATable.Columns("E").DefaultValue = 50000.0 '设置默认值
        ATable.Columns("mu").DefaultValue = 0.3 '设置默认值 
        ATable.Columns("OnLegWeightReduceCoeff").DefaultValue = 1
        ATable.Columns("OnLegStrenthengReduceCoeff").DefaultValue = 1
        ATable.Columns("OnLegEReduceCoeff").DefaultValue = 1
        ATable.Columns("OnLegMuReduceCoeff").DefaultValue = 1
        '设置参数
        NewRow = ATable.Rows.Add
        NewRow("ID") = 1
        NewRow("Name") = "淤泥"

        NewRow("Su") = "0,10;-5,12;-10,14;-15,16;-20,18"
        NewRow("Su0") = 10
        NewRow("DSu") = 2
        NewRow("UnderWaterWeight") = 8
        NewRow("UnderWaterPhi") = 10
        NewRow("UnderWaterC") = 5


        NewRow = ATable.Rows.Add
        NewRow("ID") = 2
        NewRow("Name") = "淤泥质黏土"
        NewRow("Su") = "0,10;-5,12;-10,14;-15,16;-20,18"
        NewRow("Su0") = 20
        NewRow("DSu") = 2
        NewRow("UnderWaterWeight") = 8
        NewRow("UnderWaterPhi") = 10
        NewRow("UnderWaterC") = 15

        NewRow = ATable.Rows.Add
        NewRow("ID") = 3
        NewRow("Name") = "黏土"
        NewRow("Su") = "0,10;-5,12;-10,14;-15,16;-20,18"
        NewRow("Su0") = 30
        NewRow("DSu") = 2
        NewRow("UnderWaterWeight") = 8
        NewRow("UnderWaterPhi") = 25
        NewRow("UnderWaterC") = 0

        NewRow = ATable.Rows.Add
        NewRow("ID") = 4
        NewRow("Name") = "砂砾"
        NewRow("Su") = "0,10;-5,12;-10,14;-15,16;-20,18"
        NewRow("Su0") = 40
        NewRow("DSu") = 2
        NewRow("UnderWaterWeight") = 8
        NewRow("UnderWaterPhi") = 25
        NewRow("UnderWaterC") = 0

        '钻孔
        ATable = New DataTable("LS_SoilDrilling") '多钻孔计算模式
        MydataSet.Tables.Add(ATable)
        With ATable.Columns
            .Add("ID", System.Type.GetType("System.Int32")) '孔号
            .Add("Name", System.Type.GetType("System.String")) '钻孔名称
            .Add("x", System.Type.GetType("System.Double")) '全局坐标x
            .Add("y", System.Type.GetType("System.Double")) '全局坐标y
            .Add("SoilLayers", System.Type.GetType("System.String")) '地层参数’，文本表示（土层名,层顶高程），例如：一个钻孔表示为 '淤泥',1;'淤泥质黏土',-5;'黏土',-15;'砂砾',-20
        End With
        ATable.Columns("ID").DefaultValue = 0
        ATable.Columns("Name").DefaultValue = ""
        ATable.Columns("x").DefaultValue = 0
        ATable.Columns("y").DefaultValue = 0
        ATable.Columns("SoilLayers").DefaultValue = ""


        NewRow = ATable.Rows.Add
        NewRow("ID") = 1
        NewRow("Name") = "钻孔#1"
        NewRow("x") = -5
        NewRow("y") = -5
        NewRow("SoilLayers") = "淤泥,-2.5;淤泥质黏土,-12.5;黏土,-25;砂砾,-45"

        NewRow = ATable.Rows.Add
        NewRow("ID") = 2
        NewRow("Name") = "钻孔#2"
        NewRow("x") = 0
        NewRow("y") = -5
        NewRow("SoilLayers") = "淤泥,-2.5;淤泥质黏土,-12.5;黏土,-25;砂砾,-45"

        NewRow = ATable.Rows.Add
        NewRow("ID") = 3
        NewRow("Name") = "钻孔#3"
        NewRow("x") = 0
        NewRow("y") = -5
        NewRow("SoilLayers") = "淤泥,-2.5;淤泥质黏土,-12.5;黏土,-25;砂砾,-45"


        NewRow = ATable.Rows.Add
        NewRow("ID") = 4
        NewRow("Name") = "钻孔#4"
        NewRow("x") = -5
        NewRow("y") = 5
        NewRow("SoilLayers") = "淤泥,-2.5;淤泥质黏土,-12.5;黏土,-25;砂砾,-45"

        NewRow = ATable.Rows.Add
        NewRow("ID") = 5
        NewRow("Name") = "钻孔#5"
        NewRow("x") = 0
        NewRow("y") = 5
        NewRow("SoilLayers") = "淤泥,-2.5;淤泥质黏土,-12.5;黏土,-25;砂砾,-45"

        NewRow = ATable.Rows.Add
        NewRow("ID") = 6
        NewRow("Name") = "钻孔#6"
        NewRow("x") = 0
        NewRow("y") = 5
        NewRow("SoilLayers") = "淤泥,-2.5;淤泥质黏土,-12.5;黏土,-25;砂砾,-45"


        '当前腿位置处的土层
        ATable = New DataTable("LS_LegSoilLayer") '单钻孔计算模式
        MydataSet.Tables.Add(ATable)
        With ATable.Columns
            .Add("DrillingID", System.Type.GetType("System.Int32")) '钻孔号
            .Add("DrillingName", System.Type.GetType("System.String")) '钻孔名称
            .Add("SoilID", System.Type.GetType("System.Int32")) '土层编号
            .Add("TopLevel", System.Type.GetType("System.Double")) '顶层高程
        End With
        ATable.Columns("DrillingID").DefaultValue = 1
        ATable.Columns("DrillingName").DefaultValue = "钻孔#1"
        ATable.Columns("SoilID").DefaultValue = 0
        ATable.Columns("TopLevel").DefaultValue = 0

        NewRow = ATable.Rows.Add
        NewRow("DrillingID") = 1
        NewRow("DrillingName") = "钻孔#1"
        NewRow("SoilID") = 1
        NewRow("TopLevel") = -2.5
    End Sub


    Sub CreateComponentTable(Optional ByVal Boats As Boolean = True)
        Dim ATable As DataTable
        Dim NewRow As DataRow
        'Dim MyDataSet As DataSet = Application.GetStructureKit.GetData

        '桩腿结构类型
        ATable = New DataTable("LS_LegType")
        MydataSet.Tables.Add(ATable)
        With ATable.Columns
            .Add("ID", System.Type.GetType("System.Int32"))
            .Add("Name", System.Type.GetType("System.String"))
        End With
        NewRow = ATable.Rows.Add
        NewRow("ID") = 1
        NewRow("Name") = "圆柱式"
        NewRow = ATable.Rows.Add
        NewRow("ID") = 2
        NewRow("Name") = "桁架式"
        '桩靴结构类型
        ATable = New DataTable("LS_SpudcanType")
        MydataSet.Tables.Add(ATable)
        With ATable.Columns
            .Add("ID", System.Type.GetType("System.Int32"))
            .Add("Name", System.Type.GetType("System.String"))
        End With
        NewRow = ATable.Rows.Add
        NewRow("ID") = 0
        NewRow("Name") = "类圆形"
        NewRow = ATable.Rows.Add
        NewRow("ID") = 1
        NewRow("Name") = "类四边形"
        'For i = 1 To 8
        '    NewRow = ATable.Rows.Add
        '    NewRow("ID") = i
        '    NewRow("Name") = "Type" & i
        'Next
        If Boats Then
            CreateBoatTable()
            Exit Sub
        End If

        '桩腿
        ATable = New DataTable("LS_Leg")
        MydataSet.Tables.Add(ATable)
        With ATable.Columns
            .Add("ID", System.Type.GetType("System.Int32")) '采用自数据库时候ID
            .Add("Type", System.Type.GetType("System.Int32")) '类型，圆形-1,桁架-2
            .Add("B", System.Type.GetType("System.Double")) '等效宽度
            .Add("Circumference", System.Type.GetType("System.Double")) '等效周长
            .Add("Diameter", System.Type.GetType("System.Double")) '等效直径
            .Add("Area", System.Type.GetType("System.Double")) '等效截面积
            .Add("Volume0", System.Type.GetType("System.Double")) '每延米毛体积
            .Add("Volume", System.Type.GetType("System.Double")) '每延米体积
            .Add("Weight", System.Type.GetType("System.Double")) '每延米重量(kN)
            .Add("Parameter", System.Type.GetType("System.String")) '结构尺寸参数*****
            .Add("TopLevel", System.Type.GetType("System.Double")) '顶部高程
            .Add("ActiveLength", System.Type.GetType("System.Double")) '有效长度(m)
        End With
        '桩腿分圆柱的和桁架的类型,圆柱类型直径一般在2-3.6米
        NewRow = ATable.Rows.Add
        NewRow("ID") = 1
        NewRow("Type") = 1
        NewRow("Circumference") = 3
        NewRow("Diameter") = 2
        NewRow("Area") = 0
        NewRow("Volume0") = 0
        NewRow("Volume") = 0
        NewRow("Weight") = 1
        NewRow("Parameter") = ""
        NewRow("TopLevel") = 0
        NewRow("ActiveLength") = 50
        '桩靴
        ATable = New DataTable("LS_Spudcan")
        MydataSet.Tables.Add(ATable)
        With ATable.Columns
            .Add("ID", System.Type.GetType("System.Int32")) '采用自数据库时候ID
            .Add("Type", System.Type.GetType("System.Int32")) '类四边形-1,类圆形-3
            .Add("ShapeType", System.Type.GetType("System.Int32")) '方形-1,圆形-0
            .Add("B", System.Type.GetType("System.Double")) '等效宽度
            .Add("L", System.Type.GetType("System.Double")) '等效长度
            .Add("Circumference", System.Type.GetType("System.Double")) '最大处周长
            .Add("Diameter", System.Type.GetType("System.Double")) '等效直径
            .Add("Area", System.Type.GetType("System.Double")) '面积
            .Add("Weight", System.Type.GetType("System.Double")) '重量(kN)
            .Add("Volume", System.Type.GetType("System.Double")) '体积(m^3)
            .Add("Parameter", System.Type.GetType("System.String")) '结构尺寸参数***** 
        End With
        '桩靴尺寸一般在长度10-15米,宽度7-14米,高度2-3米
        NewRow = ATable.Rows.Add
        NewRow("ID") = 1
        NewRow("Type") = 3
        NewRow("ShapeType") = 0
        NewRow("B") = 5
        NewRow("L") = 5
        NewRow("Diameter") = 5
        NewRow("Circumference") = 15.7
        NewRow("Area") = 19.26
        NewRow("Volume") = 40
        NewRow("Weight") = 1000
        NewRow("Parameter") = "D=6,H1=0.5,H2=0.5,H3=0.5"
    End Sub

    Sub CreateBoatTable()
        Dim ATable As DataTable
        Dim NewRow As DataRow
        ATable = New DataTable("LS_Boat")
        MydataSet.Tables.Add(ATable)
        With ATable.Columns
            .Add("IsCount", System.Type.GetType("System.Boolean")) '是否参与计算
            .Add("ID", System.Type.GetType("System.Int32"))
            .Add("Name", System.Type.GetType("System.String"))
            .Add("W", System.Type.GetType("System.Double")) '桩腿、桩靴自重(t)
            .Add("SumW", System.Type.GetType("System.Double")) '计算预压荷载'计算总重力(t)
            .Add("PullingCapacity", System.Type.GetType("System.Double")) '拔桩力(t)
            .Add("GroundPressure", System.Type.GetType("System.Double")) '对地比压(kPa)
            .Add("AirGap", System.Type.GetType("System.Double")) '气隙（船底到水面）(m)
            .Add("LegPressForce", System.Type.GetType("System.Double")) '桩腿预压力(t)
            '桩腿
            .Add("LegType", System.Type.GetType("System.Int32")) '类型，圆形-1,桁架-2
            .Add("LegDiameter", System.Type.GetType("System.Double")) '桩腿直径(m)
            .Add("LegCircumference", System.Type.GetType("System.Double")) '桩腿周长(m)
            .Add("LegHLN", System.Type.GetType("System.Double")) '有效桩腿长度（船底到靴底）：最大工作长度=桩腿总长-裕量-固桩架/升降室-型深D-气息的长度
            .Add("LegActiveLength", System.Type.GetType("System.Double")) '桩腿有效长度(m)
            .Add("LegA", System.Type.GetType("System.Double")) '桩腿面积(m2)
            '桩靴
            .Add("SpudcanShapeType", System.Type.GetType("System.Int32")) '方形-1,圆形-0
            .Add("SpudcanL", System.Type.GetType("System.String")) '桩靴长度(m)
            .Add("SpudcanB", System.Type.GetType("System.Double")) '桩靴宽度或直径(m) 
            .Add("SpudcanParameter", System.Type.GetType("System.String")) '桩靴形状参数
            .Add("SpudcanA", System.Type.GetType("System.Double")) '桩靴面积(m2)
            .Add("SpudcanCircumference", System.Type.GetType("System.Double")) '桩靴最大截面周长(m)
            .Add("SpudcanV", System.Type.GetType("System.Double")) '桩靴体积(m3) 
        End With
        ATable.Columns("IsCount").DefaultValue = 1 '设置默认值
        ATable.Columns("ID").DefaultValue = 1 '设置默认值
        ATable.Columns("Name").DefaultValue = "船1" '设置默认值
        ATable.Columns("W").DefaultValue = 800 '设置默认值
        ATable.Columns("SumW").DefaultValue = 5000 '设置默认值
        ATable.Columns("PullingCapacity").DefaultValue = 3500 '设置默认值
        ATable.Columns("GroundPressure").DefaultValue = 500 '设置默认值
        ATable.Columns("AirGap").DefaultValue = 5 '设置默认值
        ATable.Columns("LegType").DefaultValue = 1 '设置默认值
        ATable.Columns("LegDiameter").DefaultValue = 4 '设置默认值
        ATable.Columns("LegCircumference").DefaultValue = 30 '设置默认值
        ATable.Columns("LegPressForce").DefaultValue = 4500 '设置默认值
        ATable.Columns("LegHLN").DefaultValue = 50 '设置默认值* *
        ATable.Columns("LegActiveLength").DefaultValue = 50 '设置默认值
        ATable.Columns("LegA").DefaultValue = 0 '设置默认值
        ATable.Columns("SpudcanShapeType").DefaultValue = 1 '设置默认值
        ATable.Columns("SpudcanL").DefaultValue = 16 '设置默认值
        ATable.Columns("SpudcanB").DefaultValue = 16 '设置默认值 
        ATable.Columns("SpudcanParameter").DefaultValue = "H2=2" '设置默认值
        ATable.Columns("SpudcanA").DefaultValue = 201.06 '设置默认值
        ATable.Columns("SpudcanCircumference").DefaultValue = 50.26 '设置默认值
        ATable.Columns("SpudcanV").DefaultValue = 804.25 '设置默认值

        NewRow = ATable.Rows.Add
    End Sub
    Sub CreateStructureParameterTable()
        Dim ATable As DataTable
        Dim NewRow As DataRow
        'Dim MyDataSet As DataSet = Application.GetStructureKit.GetData
        ATable = New DataTable("LS_StructureData") '竖向地基承载力计算参数
        With ATable.Columns
            .Add("WindFieldName", System.Type.GetType("System.String")) '风场名 
            .Add("BoatName", System.Type.GetType("System.String")) '船名
            .Add("PullingCapacity", System.Type.GetType("System.Double")) '拔桩能力(t)-船舶的参数
            .Add("UserName", System.Type.GetType("System.String")) '计算人姓名
            .Add("ContactNumber", System.Type.GetType("System.Int64")) '联系方式
            .Add("WindFieldWaterHeight", System.Type.GetType("System.Double")) '风场区域水深(m)（17-19）
            .Add("AirGap", System.Type.GetType("System.Double")) '气隙(m)（5）
            .Add("GetJettingSystem", System.Type.GetType("System.Boolean")) '冲桩系统是否具备
            .Add("GoodWorking", System.Type.GetType("System.Boolean")) '工作状态是否良好
        End With
        MydataSet.Tables.Add(ATable)
        NewRow = ATable.Rows.Add
        NewRow("WindFieldName") = "风场1"
        NewRow("BoatName") = "船1"
        NewRow("PullingCapacity") = 3500
        NewRow("UserName") = "计算人甲"
        NewRow("ContactNumber") = 12345678900
        NewRow("WindFieldWaterHeight") = 17 '17-19
        NewRow("AirGap") = 5
        NewRow("GetJettingSystem") = True
        NewRow("GoodWorking") = True
        ATable = New DataTable("LS_CalculationParameter") '竖向地基承载力计算参数
        With ATable.Columns
            .Add("CalculationMethod", System.Type.GetType("System.Int32")) '计算方法，1-公式法,2-有限元法
            .Add("IsEquivalentToCircleSpudcan", System.Type.GetType("System.Boolean")) '（砂土）是否等效为圆形桩靴
            .Add("UnderWaterPhiSubtractValue", System.Type.GetType("System.Double")) '砂土内摩擦角降低度数
            .Add("DestinationLevel", System.Type.GetType("System.Double")) '最大计算高程
            .Add("NCalculatePoint", System.Type.GetType("System.Int32")) '计算高程点数量
            .Add("IsSealed", System.Type.GetType("System.Boolean")) '桩靴是否密封扣除浮力
            '多船版本删除
            .Add("PressForce", System.Type.GetType("System.Double")) '计算预压荷载(t)，同LS_Boat中的SumW
            .Add("GroundPressure", System.Type.GetType("System.Double")) '对地比压(kPa)，同LS_Boat中的GroundPressure，用来画图
            '有限元法参数
            .Add("MeshSize", System.Type.GetType("System.Double")) '计算单元尺寸
            .Add("DPType", System.Type.GetType("System.Int32")) '使用的DP准则
            .Add("KeepHistory", System.Type.GetType("System.Boolean")) '保留计算结果
            .Add("DCoeff", System.Type.GetType("System.Double")) '系数收敛
            .Add("Hc2", System.Type.GetType("System.Double")) '有限元时候的洞口深度
            .Add("cohesionCoeff", System.Type.GetType("System.Double")) '桩底粘结系数
            '抗压
            .Add("IsBackFlow", System.Type.GetType("System.Boolean")) '考虑回流
            .Add("AutoGetHc", System.Type.GetType("System.Boolean")) '自动计算极限孔洞深度Hc
            .Add("Hc", System.Type.GetType("System.Double"))

            '抗拉
            .Add("ftop", System.Type.GetType("System.Double")) '土体强度折减系数，桩靴上部土体因扰动产生的强度降低，与工作时间相关
            .Add("fbase", System.Type.GetType("System.Double")) '强度增长系数，桩靴下部土体在荷载作用下再固结而产生强度增加，与工作时间相关
            .Add("NBreakout", System.Type.GetType("System.Double")) '突破系数
            .Add("SoilCoarseCoeff", System.Type.GetType("System.Double")) '粗糙度系数
            .Add("fb", System.Type.GetType("System.Double")) '冲桩减阻系数fb
            .Add("fleg", System.Type.GetType("System.Double")) '桩腿侧摩阻力系数fleg，取值范围0~1，默认取0


        End With
        ATable.Columns("CalculationMethod").DefaultValue = 1
        ATable.Columns("IsEquivalentToCircleSpudcan").DefaultValue = True
        ATable.Columns("UnderWaterPhiSubtractValue").DefaultValue = 5
        ATable.Columns("IsSealed").DefaultValue = True
        ATable.Columns("DestinationLevel").DefaultValue = -20
        ATable.Columns("NCalculatePoint").DefaultValue = 1 '默认为1，即计算每个地层分界面位置
        ATable.Columns("PressForce").DefaultValue = 4500
        ATable.Columns("GroundPressure").DefaultValue = 233.64
        ATable.Columns("MeshSize").DefaultValue = 1
        ATable.Columns("DPType").DefaultValue = 4
        ATable.Columns("KeepHistory").DefaultValue = 1
        ATable.Columns("DCoeff").DefaultValue = 0.001
        ATable.Columns("IsBackFlow").DefaultValue = 1
        ATable.Columns("AutoGetHc").DefaultValue = False '默认按全回流高度计算，Hc=0
        ATable.Columns("Hc").DefaultValue = 0
        ATable.Columns("Hc2").DefaultValue = 0 '有限元时候的洞口深度
        ATable.Columns("cohesionCoeff").DefaultValue = 1
        ATable.Columns("ftop").DefaultValue = 1
        ATable.Columns("fbase").DefaultValue = 1
        ATable.Columns("NBreakout").DefaultValue = 8
        ATable.Columns("SoilCoarseCoeff").DefaultValue = 0.5
        ATable.Columns("fb").DefaultValue = 0
        ATable.Columns("fleg").DefaultValue = 0.5


        MydataSet.Tables.Add(ATable)
        NewRow = ATable.Rows.Add
        NewRow("CalculationMethod") = 1
        NewRow("DestinationLevel") = -20
        NewRow("NCalculatePoint") = 20
        NewRow("PressForce") = 4500
        NewRow("MeshSize") = 1
        NewRow("DPType") = 4
        NewRow("KeepHistory") = 1
        NewRow("DCoeff") = 0.001
        NewRow("IsBackFlow") = 1
        NewRow("AutoGetHc") = True
        NewRow("Hc") = 0
        NewRow("ftop") = 1
        NewRow("fbase") = 1
        NewRow("NBreakout") = 1
        NewRow("SoilCoarseCoeff") = 0.5
        NewRow("fb") = 0
    End Sub
    Sub CreateMeshTable()
        Dim ATable As DataTable
        'Dim MyDataSet As DataSet = Application.GetStructureKit.GetData
        '有限元计算区域
        ATable = New DataTable("LS_CalculationMaterials") '围成区域的组成
        With ATable.Columns
            .Add("ID", System.Type.GetType("System.Int32")) '材料ID
            .Add("Name", System.Type.GetType("System.String")) '材料名称
            .Add("Type", System.Type.GetType("System.Int32")) '材料类型
            .Add("ModeType", System.Type.GetType("System.Int32")) '材料DP类型
            .Add("ElasticPlasticType", System.Type.GetType("System.Int32")) '弹塑性类型
            .Add("E", System.Type.GetType("System.Double")) '弹性模量E
            .Add("Mu", System.Type.GetType("System.Double")) '泊松比
            .Add("Phi", System.Type.GetType("System.Double")) '摩擦角
            .Add("C", System.Type.GetType("System.Double")) '粘结力
            .Add("FlowAngle", System.Type.GetType("System.Double")) '膨胀角
            .Add("Weight", System.Type.GetType("System.Double")) '重度
        End With
        MydataSet.Tables.Add(ATable)
        ATable.Columns("ID").DefaultValue = 0
        ATable.Columns("Name").DefaultValue = 0
        ATable.Columns("ModeType").DefaultValue = 0
        ATable.Columns("ElasticPlasticType").DefaultValue = 0
        ATable.Columns("E").DefaultValue = 0
        ATable.Columns("Phi").DefaultValue = 0
        ATable.Columns("C").DefaultValue = 0
        ATable.Columns("FlowAngle").DefaultValue = 0
        ATable.Columns("Weight").DefaultValue = 0



        ATable = New DataTable("LS_CalculationLevels") '围成区域的点
        With ATable.Columns
            .Add("DrillingID", System.Type.GetType("System.Int32")) '钻孔号 
            .Add("LevelID", System.Type.GetType("System.Int32")) '计算高程ID
            .Add("Level", System.Type.GetType("System.Double")) '计算底部高程
            .Add("SelectMode_Qv", System.Type.GetType("System.Int32")) '承载力选择计算模式
            .Add("SelectMode_Qb", System.Type.GetType("System.Int32")) '拔桩力选择计算模式
        End With
        MydataSet.Tables.Add(ATable)
        ATable.Columns("DrillingID").DefaultValue = 1
        ATable.Columns("LevelID").DefaultValue = 0
        ATable.Columns("Level").DefaultValue = 0
        ATable.Columns("SelectMode_Qv").DefaultValue = 0
        ATable.Columns("SelectMode_Qb").DefaultValue = 0



        ATable = New DataTable("LS_CalculationNodes") '围成区域的点
        With ATable.Columns
            .Add("LevelID", System.Type.GetType("System.Int32")) '计算高程ID
            .Add("NodeID", System.Type.GetType("System.Int32")) '计算节点ID
            .Add("x", System.Type.GetType("System.Double")) 'x
            .Add("y", System.Type.GetType("System.Double")) 'x
        End With
        MydataSet.Tables.Add(ATable)
        ATable.Columns("LevelID").DefaultValue = 0
        ATable.Columns("NodeID").DefaultValue = 0
        ATable.Columns("x").DefaultValue = 0
        ATable.Columns("y").DefaultValue = 0



        ATable = New DataTable("LS_CalculationEdges") '围成区域的边
        With ATable.Columns
            .Add("LevelID", System.Type.GetType("System.Int32")) '计算高程ID
            .Add("EdgeID", System.Type.GetType("System.Int32")) '计算区域ID
            .Add("x1", System.Type.GetType("System.Double")) 'x1
            .Add("y1", System.Type.GetType("System.Double")) 'y1
            .Add("x2", System.Type.GetType("System.Double")) 'x2
            .Add("y2", System.Type.GetType("System.Double")) 'y2
            .Add("SupportID", System.Type.GetType("System.Int32")) '边界的支撑ID
            .Add("ReleaseID", System.Type.GetType("System.Int32")) '边界的自由度释放ID
            .Add("Type", System.Type.GetType("System.Int32")) '0表示土层上部,1表示中心线,2表示计算边界
            .Add("MeshNodes", System.Type.GetType("System.String")) '组成边界上的点
        End With
        MydataSet.Tables.Add(ATable)
        ATable.Columns("LevelID").DefaultValue = 0
        ATable.Columns("EdgeID").DefaultValue = 0
        ATable.Columns("x1").DefaultValue = 0
        ATable.Columns("y1").DefaultValue = 0
        ATable.Columns("x2").DefaultValue = 0
        ATable.Columns("y2").DefaultValue = 0
        ATable.Columns("SupportID").DefaultValue = 0
        ATable.Columns("ReleaseID").DefaultValue = 0
        ATable.Columns("MeshNodes").DefaultValue = ""




        ATable = New DataTable("LS_CalculationAreas") '围成区域的组成
        With ATable.Columns
            .Add("LevelID", System.Type.GetType("System.Int32")) '计算高程ID
            .Add("AreaID", System.Type.GetType("System.Int32")) '计算区域ID
            .Add("MaterialID", System.Type.GetType("System.Int32")) '材料ID
            .Add("BeforeMaterialID", System.Type.GetType("System.Int32")) '材料ID
            .Add("Edges", System.Type.GetType("System.String")) '组成区域的边界
            .Add("x0", System.Type.GetType("System.Double")) 'x1
            .Add("y0", System.Type.GetType("System.Double")) 'y1
            .Add("Location", System.Type.GetType("System.Int32")) '与桩靴位置关系，1在上面，2 在下面
        End With
        MydataSet.Tables.Add(ATable)
        ATable.Columns("LevelID").DefaultValue = 0
        ATable.Columns("AreaID").DefaultValue = 0
        ATable.Columns("MaterialID").DefaultValue = 0
        ATable.Columns("BeforeMaterialID").DefaultValue = 0
        ATable.Columns("Edges").DefaultValue = ""
        ATable.Columns("x0").DefaultValue = 0
        ATable.Columns("y0").DefaultValue = 0
        ATable.Columns("Location").DefaultValue = 1
        ATable = New DataTable("LS_MeshNodes") '有限元的节点
        With ATable.Columns
            .Add("LevelID", System.Type.GetType("System.Int32")) '计算高程ID
            .Add("NodeID", System.Type.GetType("System.Int32")) '节点ID
            .Add("x", System.Type.GetType("System.Double")) 'x
            .Add("y", System.Type.GetType("System.Double")) 'y
        End With
        MydataSet.Tables.Add(ATable)
        ATable.Columns("LevelID").DefaultValue = 0
        ATable.Columns("NodeID").DefaultValue = 0
        ATable.Columns("x").DefaultValue = 0
        ATable.Columns("y").DefaultValue = 0

        ATable = New DataTable("LS_AreaMeshs") '围有限元的网格
        With ATable.Columns
            .Add("LevelID", System.Type.GetType("System.Int32")) '计算高程ID
            .Add("AreaID", System.Type.GetType("System.Int32")) '计算区域ID
            .Add("MeshID", System.Type.GetType("System.Int32")) '计算网格ID
            .Add("MeshType", System.Type.GetType("System.Int32")) '网格类型
            .Add("N1", System.Type.GetType("System.Int32")) '节点N1
            .Add("N2", System.Type.GetType("System.Int32")) '节点N2
            .Add("N3", System.Type.GetType("System.Int32")) '节点N3
            .Add("N4", System.Type.GetType("System.Int32")) '节点N3
        End With
        MydataSet.Tables.Add(ATable)
        ATable.Columns("LevelID").DefaultValue = 0
        ATable.Columns("AreaID").DefaultValue = 0
        ATable.Columns("MeshID").DefaultValue = 0
        ATable.Columns("MeshType").DefaultValue = 10
        ATable.Columns("N1").DefaultValue = 0
        ATable.Columns("N2").DefaultValue = 0
        ATable.Columns("N3").DefaultValue = 0
        ATable.Columns("N4").DefaultValue = 0
        ATable = New DataTable("LS_InfiniteMeshs") '无限元
        With ATable.Columns
            .Add("LevelID", System.Type.GetType("System.Int32")) '计算高程ID
            .Add("MeshID", System.Type.GetType("System.Int32")) '单元ID
            .Add("N1", System.Type.GetType("System.Int32")) '节点N1
            .Add("N2", System.Type.GetType("System.Int32")) '节点N2
            .Add("N3", System.Type.GetType("System.Int32")) '节点N3
            .Add("N4", System.Type.GetType("System.Int32")) '节点N4
            .Add("N5", System.Type.GetType("System.Int32")) '节点N5
            .Add("N6", System.Type.GetType("System.Int32")) '节点N6
        End With
        MydataSet.Tables.Add(ATable)
        ATable.Columns("LevelID").DefaultValue = 0
        ATable.Columns("MeshID").DefaultValue = 0
        ATable.Columns("N1").DefaultValue = 0
        ATable.Columns("N2").DefaultValue = 0
        ATable.Columns("N3").DefaultValue = 0
        ATable.Columns("N4").DefaultValue = 0
        ATable.Columns("N5").DefaultValue = 0
        ATable.Columns("N6").DefaultValue = 0

        ATable = New DataTable("LS_CoupleNodes") '耦合节点
        With ATable.Columns
            .Add("LevelID", System.Type.GetType("System.Int32")) '计算高程ID
            .Add("CoupleNodeID", System.Type.GetType("System.Int32")) '节点ID
            .Add("N1", System.Type.GetType("System.Int32")) '节点ID
            .Add("N2", System.Type.GetType("System.Int32")) '节点ID
            .Add("CoupleID", System.Type.GetType("System.Int32")) '节点ID
        End With
        MydataSet.Tables.Add(ATable)
        ATable.Columns("LevelID").DefaultValue = 0
        ATable.Columns("CoupleNodeID").DefaultValue = 0
        ATable.Columns("N1").DefaultValue = 0
        ATable.Columns("N2").DefaultValue = 0
        ATable.Columns("CoupleID").DefaultValue = 0


        ATable = New DataTable("LS_Contactors") '接触单元
        With ATable.Columns
            .Add("LevelID", System.Type.GetType("System.Int32")) '计算高程ID
            .Add("ID", System.Type.GetType("System.Int32")) 'ID
            .Add("ContactorID", System.Type.GetType("System.Int32")) '接触单元
            .Add("Edge1s", System.Type.GetType("System.String")) '主动接触的边
            .Add("Edge2s", System.Type.GetType("System.String")) '被动接触的边
            .Add("Nodes1", System.Type.GetType("System.String"))
            .Add("Nodes2", System.Type.GetType("System.String"))
            .Add("LocalCoordinate", System.Type.GetType("System.String"))
        End With
        MydataSet.Tables.Add(ATable)
        ATable.Columns("LevelID").DefaultValue = 0
        ATable.Columns("ID").DefaultValue = 0
        ATable.Columns("ContactorID").DefaultValue = 0
        ATable.Columns("Edge1s").DefaultValue = ""
        ATable.Columns("Edge2s").DefaultValue = ""
        ATable.Columns("Nodes1").DefaultValue = ""
        ATable.Columns("Nodes2").DefaultValue = ""
        ATable.Columns("LocalCoordinate").DefaultValue = ""

    End Sub
    Sub CreateResultTable()
        Dim ATable As DataTable
        Dim NewRow As DataRow
        'Dim MyDataSet As DataSet = Application.GetStructureKit.GetData
        ATable = New DataTable("LS_Load")
        With ATable.Columns
            .Add("ID", System.Type.GetType("System.Int32")) '荷载编号
            .Add("Name", System.Type.GetType("System.String")) '荷载名称
        End With
        MydataSet.Tables.Add(ATable)
        NewRow = ATable.Rows.Add()
        NewRow("ID") = 1
        NewRow("Name") = "初始应力场"


        NewRow = ATable.Rows.Add()
        NewRow("ID") = 2
        NewRow("Name") = "极限压力"


        NewRow = ATable.Rows.Add()
        NewRow("ID") = 4
        NewRow("Name") = "极限拔力"





        '计算方法执行标准为BSENISO19905-1-2016
        '极限洞深度
        ATable = New DataTable("LS_Holl")
        MydataSet.Tables.Add(ATable)
        With ATable.Columns
            .Add("DrillingID", System.Type.GetType("System.Int32")) '钻孔号
            .Add("Hc", System.Type.GetType("System.Double")) '极限孔洞深度
            .Add("Su", System.Type.GetType("System.Double")) '极限孔洞深度处的不排水抗剪强度
            .Add("SuH", System.Type.GetType("System.Double")) '极限孔洞深度处的不排水抗剪强度
            .Add("Sum", System.Type.GetType("System.Double")) '海床面处的不排水抗剪强度
            .Add("Rho", System.Type.GetType("System.Double")) '不排水抗剪强度的增加速率
            .Add("SoilWeight", System.Type.GetType("System.Double")) '粘土的浮重度
        End With
        ATable.Columns("DrillingID").DefaultValue = 1

        '抗压承载力标准值
        ATable = New DataTable("LS_PressResistanceResult")
        MydataSet.Tables.Add(ATable)
        With ATable.Columns
            .Add("DrillingID", System.Type.GetType("System.Int32")) '钻孔号
            .Add("ID", System.Type.GetType("System.Int32")) '计算高程ID
            .Add("Level", System.Type.GetType("System.Double")) '计算高程
            .Add("SelectMode", System.Type.GetType("System.Int32")) '选择计算模式
            .Add("QvP", System.Type.GetType("System.String")) '选择计算模式下的地基承载力(kPa)
            .Add("Qv", System.Type.GetType("System.String")) '选择计算模式下的地基承载力(kN)
            .Add("Qv1", System.Type.GetType("System.String")) '常规破坏(暂不使用)
            .Add("Qv1_Sand", System.Type.GetType("System.String")) '常规破坏(砂土)
            .Add("Qv1_Clay", System.Type.GetType("System.String")) '常规破坏(黏土)
            .Add("Qv2", System.Type.GetType("System.String")) '挤出破坏结果
            .Add("Qv3", System.Type.GetType("System.String")) '穿刺破坏(暂不使用)
            .Add("Qv3_Sand", System.Type.GetType("System.String")) '穿刺破坏(砂土穿刺黏土)
            .Add("Qv3_Clay", System.Type.GetType("System.String")) '穿刺破坏(黏土穿刺黏土)
            .Add("Qv4", System.Type.GetType("System.String")) '分层土破坏
            .Add("Description", System.Type.GetType("System.String")) '公式描述
            '中间结果
            .Add("SoilID", System.Type.GetType("System.Int32"))
            .Add("IsSand", System.Type.GetType("System.Boolean")) '是否砂土
            .Add("Parameter_D", System.Type.GetType("System.Double")) '最大插深，最大承载面到海床面的距离
            .Add("Parameter_B", System.Type.GetType("System.Double")) '有效承载面的宽度或直径。圆形桩靴时为有效承载截面直径；矩形桩靴时为有效承载截面短边长度
            .Add("Parameter_A", System.Type.GetType("System.Double")) '桩靴最大承载截面
            .Add("Parameter_p0", System.Type.GetType("System.Double")) '桩靴最大承载截面处的有效上覆土压力
            '黏土层(单一土层)
            .Add("Parameter_Su", System.Type.GetType("System.Double")) '土体未扰动不排水抗剪强度
            .Add("Parameter_Nc", System.Type.GetType("System.Double")) '承载力系数
            .Add("Parameter_Sc", System.Type.GetType("System.Double")) '承载力深度系数
            .Add("Parameter_dc", System.Type.GetType("System.Double")) '承载力深度系数
            '砂土层(单一土层)
            .Add("Parameter_weight", System.Type.GetType("System.Double")) '土层浮重度
            .Add("Parameter_dgamma", System.Type.GetType("System.Double")) '土层排水条件下，超载的深度修正系数
            .Add("Parameter_Ngamma", System.Type.GetType("System.Double")) '承载力系数
            .Add("Parameter_Nqamma", System.Type.GetType("System.Double")) '承载力系数
            .Add("Parameter_dq", System.Type.GetType("System.Double")) '承载力深度修正系数
            '挤出破坏(软土下有硬土)
            .Add("Parameter_as", System.Type.GetType("System.Double")) '系数，取5.00
            .Add("Parameter_bs", System.Type.GetType("System.Double")) '系数，取0.33
            .Add("Parameter_T", System.Type.GetType("System.Double")) '软粘土层厚度
            '穿刺破坏(硬土下有软土)
            .Add("Parameter_H", System.Type.GetType("System.Double")) '桩靴底到软弱土层的距离
            .Add("Parameter_Sut", System.Type.GetType("System.Double")) '硬粘土层的抗剪强度
            .Add("Parameter_Sub", System.Type.GetType("System.Double")) '软粘土层的抗剪强度
        End With
        For Each AColumn As DataColumn In ATable.Columns
            If AColumn.DataType = Type.GetType("System.Double") Or AColumn.DataType = Type.GetType("System.Int32") Or AColumn.DataType = Type.GetType("System.Boolean") Then
                AColumn.DefaultValue = 0
            End If
            If AColumn.DataType = Type.GetType("System.String") Then
                AColumn.DefaultValue = ""
            End If
        Next
        ATable.Columns("DrillingID").DefaultValue = 1
        '极限抗拔承载力标准值
        ATable = New DataTable("LS_PullResistanceResult")
        MydataSet.Tables.Add(ATable)
        With ATable.Columns
            .Add("DrillingID", System.Type.GetType("System.Int32")) '钻孔号
            .Add("ID", System.Type.GetType("System.Int32")) '计算高程ID
            .Add("Level", System.Type.GetType("System.Double")) '计算高程
            .Add("SelectMode", System.Type.GetType("System.Int32")) '选择计算模式
            .Add("QuP", System.Type.GetType("System.String")) '抗拔力(t)
            .Add("Qu", System.Type.GetType("System.String")) '考虑冲桩减阻系数fb抗拔力(kN)
            .Add("Qu0", System.Type.GetType("System.String")) '冲桩减阻系数fb=0时抗拔力(kN)
            .Add("Qu1", System.Type.GetType("System.String")) '冲桩减阻系数fb=1时抗拔力(kN)
            .Add("Qu_Sand", System.Type.GetType("System.String")) '抗拔力
            .Add("Qu_Clay", System.Type.GetType("System.String")) '抗拔力 
            .Add("Description", System.Type.GetType("System.String")) '公式描述
            '计算参数
            .Add("DeepType", System.Type.GetType("System.Int32")) '1-浅，2-中，3-深
            .Add("DeepType_Sand", System.Type.GetType("System.Int32")) '1-浅，2-中，3-深
            .Add("DeepType_Clay", System.Type.GetType("System.Int32")) '1-浅，2-中，3-深
            .Add("SoilID", System.Type.GetType("System.Int32"))

            .Add("Su", System.Type.GetType("System.Double")) '土体不固结不排水抗剪强度(kPa)
            .Add("HColumn", System.Type.GetType("System.Double")) '桩靴最大截面处上部至海床面距离
            .Add("Vtop", System.Type.GetType("System.Double")) '桩腿体积(m3)
            .Add("LegWeight", System.Type.GetType("System.Double")) '桩腿浮重度
            .Add("SpudcanWeight", System.Type.GetType("System.Double")) '桩腿浮重度
            .Add("SoilWeightOnSpudcan", System.Type.GetType("System.Double")) '桩靴上土重度
            .Add("SoilWeightInSpudcan", System.Type.GetType("System.Double")) '桩靴处土重度
            .Add("Hc", System.Type.GetType("System.Double")) '极限孔洞深度
            '浅埋
            .Add("alpha", System.Type.GetType("System.Double")) '桩-土间粗糙度
            '深埋
            .Add("LegArea", System.Type.GetType("System.Double")) '桩的表面积
        End With
        For Each AColumn As DataColumn In ATable.Columns
            If AColumn.DataType = Type.GetType("System.Double") Or AColumn.DataType = Type.GetType("System.Int32") Or AColumn.DataType = Type.GetType("System.Boolean") Then
                AColumn.DefaultValue = 0
            ElseIf AColumn.DataType = Type.GetType("System.String") Then
                AColumn.DefaultValue = ""
            End If
        Next
        ATable.Columns("DrillingID").DefaultValue = 1
        '计算结果简表
        ATable = New DataTable("LS_DepthResult")
        MydataSet.Tables.Add(ATable)
        With ATable.Columns
            .Add("LimitForce", System.Type.GetType("System.Double")) '测试力(t)
            .Add("IsUserAdd", System.Type.GetType("System.Boolean")) 'True-写入结论，False-写入结果表
            .Add("DrillingID", System.Type.GetType("System.Int32")) '钻孔号
            .Add("SuggestedDepth", System.Type.GetType("System.Double")) '建议插深
            .Add("SupportSoilID", System.Type.GetType("System.Int32")) '持力层土
            .Add("SupportSoilStrength", System.Type.GetType("System.Double")) '持力层土强度参数(黏土是抗剪强度，砂土是内摩擦角)
            .Add("SelectMode_Qv", System.Type.GetType("System.Int32")) '抗压承载力计算模式
            .Add("Qv", System.Type.GetType("System.String")) '地基承载力(kN)
            .Add("Qu", System.Type.GetType("System.String")) '考虑冲桩减阻系数fb抗拔力(kN)
            .Add("Qu0", System.Type.GetType("System.String")) '冲桩减阻系数fb=0时抗拔力(kN)
            .Add("Qu1", System.Type.GetType("System.String")) '冲桩减阻系数fb=1时抗拔力(kN)
        End With
        For Each AColumn As DataColumn In ATable.Columns
            If AColumn.DataType = Type.GetType("System.Double") Or AColumn.DataType = Type.GetType("System.Int32") Or AColumn.DataType = Type.GetType("System.Boolean") Then
                AColumn.DefaultValue = 0
            ElseIf AColumn.DataType = Type.GetType("System.String") Then
                AColumn.DefaultValue = ""
            End If
        Next
        '穿刺相对安全系数结果20250928
        ATable = New DataTable("LS_PunctureRiskAssessmentResult")
        MydataSet.Tables.Add(ATable)
        With ATable.Columns
            .Add("DrillingID", System.Type.GetType("System.Int32")) '钻孔号
            .Add("P1", System.Type.GetType("System.String")) '对地比压
            .Add("P2", System.Type.GetType("System.String")) '持力层土强度参数(黏土是抗剪强度，砂土是内摩擦角)
            .Add("P3", System.Type.GetType("System.String")) '地基承载力(kN)
            .Add("Fs1", System.Type.GetType("System.String")) '考虑冲桩减阻系数fb抗拔力(kN)
            .Add("Fs2", System.Type.GetType("System.String")) '冲桩减阻系数fb=0时抗拔力(kN)
            .Add("IsPunctureRiskOK", System.Type.GetType("System.Boolean")) '冲桩减阻系数fb=1时抗拔力(kN)
        End With
        For Each AColumn As DataColumn In ATable.Columns
            If AColumn.DataType = Type.GetType("System.Double") Or AColumn.DataType = Type.GetType("System.Int32") Or AColumn.DataType = Type.GetType("System.Boolean") Then
                AColumn.DefaultValue = 0
            ElseIf AColumn.DataType = Type.GetType("System.String") Then
                AColumn.DefaultValue = ""
            End If
        Next

        ATable = New DataTable("LS_DeepType") '竖向地基承载力计算参数
        MydataSet.Tables.Add(ATable)
        With ATable.Columns
            .Add("ID", System.Type.GetType("System.Int32"))
            .Add("Name", System.Type.GetType("System.String"))
        End With
        Dim DeepType As String() = {"无", "浅埋", "中等埋深", "深埋"}
        For i = 0 To 3
            NewRow = ATable.Rows.Add
            NewRow("ID") = i
            NewRow("Name") = DeepType(i)
        Next

        For i = 1 To 2
            ATable = New DataTable("LS_TempDeepType" & i) '竖向地基承载力计算参数
            MydataSet.Tables.Add(ATable)
            With ATable.Columns
                .Add("ID", System.Type.GetType("System.Int32"))
                .Add("Name", System.Type.GetType("System.String"))
            End With
            For j = 0 To 3
                NewRow = ATable.Rows.Add
                NewRow("ID") = j
                NewRow("Name") = DeepType(j)
            Next
        Next

        ATable = New DataTable("LS_ComputingModelType_Qv")
        MydataSet.Tables.Add(ATable)
        With ATable.Columns
            .Add("ID", System.Type.GetType("System.Int32"))
            .Add("Name", System.Type.GetType("System.String"))
        End With
        NewRow = ATable.Rows.Add
        NewRow("ID") = 0
        NewRow("Name") = "默认" '所有模式下的最小值
        NewRow = ATable.Rows.Add
        NewRow("ID") = 1
        NewRow("Name") = "常规破坏" '常规承载力计算模式（均质土）
        NewRow = ATable.Rows.Add
        NewRow("ID") = 2
        NewRow("Name") = "分层土破坏" '常规承载力计算模式（分层土）
        NewRow = ATable.Rows.Add
        NewRow("ID") = 3
        NewRow("Name") = "挤出破坏"
        NewRow = ATable.Rows.Add
        NewRow("ID") = 4
        NewRow("Name") = "穿刺破坏"
        'NewRow = ATable.Rows.Add
        'NewRow("ID") = 5
        'NewRow("Name") = "砂土拔桩力计算"
        'NewRow = ATable.Rows.Add
        'NewRow("ID") = 6
        'NewRow("Name") = "粘土拔桩力计算"
        ATable = New DataTable("LS_ComputingModelType_Qb")
        MydataSet.Tables.Add(ATable)
        With ATable.Columns
            .Add("ID", System.Type.GetType("System.Int32"))
            .Add("Name", System.Type.GetType("System.String"))
        End With
        NewRow = ATable.Rows.Add
        NewRow("ID") = 0
        NewRow("Name") = "默认" '所有模式下的最小值
        NewRow = ATable.Rows.Add
        NewRow("ID") = 1
        NewRow("Name") = "砂土拔桩力计算"
        NewRow = ATable.Rows.Add
        NewRow("ID") = 2
        NewRow("Name") = "粘土拔桩力计算"

        ATable = New DataTable("Ls_ResultOfNodeDisplacement") '有限元计算节点位移结果
        With ATable.Columns
            .Add("LevelID", System.Type.GetType("System.Int32")) '计算高程ID
            .Add("LoadID", System.Type.GetType("System.Int32")) '荷载ID
            .Add("NodeID", System.Type.GetType("System.Int32")) '节点位移
            .Add("Ux", System.Type.GetType("System.Double")) '位移x
            .Add("Uy", System.Type.GetType("System.Double")) '位移y
        End With
        MydataSet.Tables.Add(ATable)



        ATable = New DataTable("LS_ResultOfFace") '有限元计算的应力场结果
        With ATable.Columns
            .Add("LevelID", System.Type.GetType("System.Int32")) '计算高程ID
            .Add("LoadID", System.Type.GetType("System.Int32")) '荷载ID
            .Add("FaceID", System.Type.GetType("System.Int32")) '区域ID
            .Add("NodeID", System.Type.GetType("System.Int32")) '节点ID
            '应力结果
            .Add("Sx", System.Type.GetType("System.Double"))
            .Add("Sy", System.Type.GetType("System.Double"))
            .Add("Sxy", System.Type.GetType("System.Double"))
            .Add("Sz", System.Type.GetType("System.Double"))
            '应变结果
            .Add("ex", System.Type.GetType("System.Double"))
            .Add("ey", System.Type.GetType("System.Double"))
            .Add("exy", System.Type.GetType("System.Double"))
            .Add("ez", System.Type.GetType("System.Double"))
            '塑性应变结果
            .Add("epx", System.Type.GetType("System.Double"))
            .Add("epy", System.Type.GetType("System.Double"))
            .Add("epxy", System.Type.GetType("System.Double"))
            .Add("epz", System.Type.GetType("System.Double"))
        End With
        MydataSet.Tables.Add(ATable)

    End Sub
    '获得结果的精度格式
    Friend Function GetFormat(ByVal DecimalDigitsNumber As Integer) As String
        Dim Format As String = "0."
        For i As Integer = 0 To DecimalDigitsNumber
            Format &= "0"
        Next
        Return Format
    End Function
    Function GetSoilLevel()
        Return MydataSet.Tables("LS_Soil").Compute("Max(TopLevel)", "")
    End Function
    Sub UpdateData(Optional ByVal Boats As Boolean = True)
        '对单船版本进行更新
        Update_1000()
        Update_1001()
        Update_1002()
        Update_1003()
        Update_1004()
        Update_1005()
        Update_1006()
        Update_1007()
        Update_1008()
        Update_1009()
        Update_10000(Boats) '对多船版本和单船版本不相同的部分进行更新——对多船的表进行更新
        Update_1010(Boats) '对多船版本和单船版本相同的部分进行更新——对单船的表进行更新，可能之前两个版本的表格放在一起
        Update_1011(Boats)
        Update_1012(Boats)
        Update_1013(Boats)
        Update_10001(Boats)
        Update_10002(Boats)
        Update_1014(Boats)
        Update_1015(Boats)
        Update_10003(Boats)
        '对多船版本和单船版本相同的部分和单船表进行更新，单船表的更新通过Boats来判断
        Update_1016(Boats)
        Update_1017(Boats)
        Update_1018(Boats)
        Update_1019(Boats)
        If Boats Then
            If Not MydataSet.Tables.Contains("LS_Boat") Then
                '将单船版本转为多船版本
                GetBoatsDataFromABoat()
            End If
        End If
    End Sub
    Private Sub Update_1000()
        'Dim MyDataSet As DataSet = Application.GetStructureKit.GetData
        If Double.Parse(MydataSet.Tables("Ls_Common").Rows(0).Item("BB").ToString) = 1000 Then
            With MydataSet.Tables("LS_Spudcan").Columns
                .Add("IsCoarse", System.Type.GetType("System.Boolean")) '粗糙
            End With
            MydataSet.Tables("Ls_Common").Rows(0).Item("BB") = 1001
        End If
    End Sub
    Private Sub Update_1001()
        'Dim MyDataSet As DataSet = Application.GetStructureKit.GetData
        If Double.Parse(MydataSet.Tables("Ls_Common").Rows(0).Item("BB").ToString) = 1001 Then
            With MydataSet.Tables("LS_Leg").Columns
                .Add("TopLevel", System.Type.GetType("System.Double")) '顶部高程
            End With
            MydataSet.Tables("Ls_Common").Rows(0).Item("BB") = 1002
        End If
    End Sub
    Private Sub Update_1002()
        'Dim MyDataSet As DataSet = Application.GetStructureKit.GetData
        If Double.Parse(MydataSet.Tables("Ls_Common").Rows(0).Item("BB").ToString) = 1002 Then
            With MydataSet.Tables("LS_PressResistanceResult").Columns
                .Add("QvP", System.Type.GetType("System.Double")) '顶部高程
            End With
            MydataSet.Tables("Ls_Common").Rows(0).Item("BB") = 1003
        End If
    End Sub
    Private Sub Update_1003()
        'Dim MyDataSet As DataSet = Application.GetStructureKit.GetData
        If Double.Parse(MydataSet.Tables("Ls_Common").Rows(0).Item("BB").ToString) = 1003 Then
            With MydataSet.Tables("LS_Soil").Columns
                .Add("Type", System.Type.GetType("System.Int32")) '土类型，0-粘土，1-砂土，2-不确定
            End With
            With MydataSet.Tables("LS_Leg").Columns
                .Add("Area", System.Type.GetType("System.Double")) '等效截面积
            End With
            With MydataSet.Tables("LS_CalculationParameter").Columns
                .Add("AutoGetHc", System.Type.GetType("System.Boolean")) '自动计算极限孔洞深度Hc
                .Add("Hc", System.Type.GetType("System.Double"))
            End With
            MydataSet.Tables("LS_CalculationParameter").Rows(0)("AutoGetHc") = False
            MydataSet.Tables("LS_CalculationParameter").Rows(0)("Hc") = 0
            Dim ATable As DataTable
            Dim NewRow As DataRow
            ATable = New DataTable("LS_SoilType")
            MydataSet.Tables.Add(ATable)
            With ATable.Columns
                .Add("ID", System.Type.GetType("System.Int32")) '土层ID 
                .Add("Name", System.Type.GetType("System.String")) '0-粘土，1-砂土，2-不确定 
            End With
            NewRow = ATable.Rows.Add
            NewRow("ID") = 0
            NewRow("Name") = "粘土"
            NewRow = ATable.Rows.Add
            NewRow("ID") = 1
            NewRow("Name") = "砂土"
            NewRow = ATable.Rows.Add
            NewRow("ID") = 2
            NewRow("Name") = "复合土"
            NewRow = MydataSet.Tables("LS_DeepType").Rows.Add
            NewRow("ID") = 0
            NewRow("Name") = "无"
            With MydataSet.Tables("LS_PressResistanceResult").Columns
                .Add("Qv1_Sand", System.Type.GetType("System.Double"))
                .Add("Qv1_Clay", System.Type.GetType("System.Double"))
                .Add("Qv3_Sand", System.Type.GetType("System.Double"))
                .Add("Qv3_Clay", System.Type.GetType("System.Double"))
            End With
            With MydataSet.Tables("LS_PullResistanceResult").Columns
                .Add("QuP", System.Type.GetType("System.Double")) '抗拔力
                .Add("Qu_Sand", System.Type.GetType("System.Double")) '抗拔力
                .Add("Qu_Clay", System.Type.GetType("System.Double"))
                .Add("DeepType_Sand", System.Type.GetType("System.Int32")) '0-无，1-浅，2-中，3-深
                .Add("DeepType_Clay", System.Type.GetType("System.Int32"))
            End With
            MydataSet.Tables("Ls_Common").Rows(0).Item("BB") = 1004
        End If
    End Sub
    Private Sub Update_1004()
        'Dim MyDataSet As DataSet = Application.GetStructureKit.GetData
        If Double.Parse(MydataSet.Tables("Ls_Common").Rows(0).Item("BB").ToString) = 1004 Then
            Dim ATable As DataTable
            ATable = MydataSet.Tables("LS_Contactors") '接触单元
            With ATable.Columns
                .Add("Nodes1", System.Type.GetType("System.String"))
                .Add("Nodes2", System.Type.GetType("System.String"))
                .Add("LocalCoordinate", System.Type.GetType("System.String"))
            End With

            ATable.Columns("Nodes1").DefaultValue = ""
            ATable.Columns("Nodes2").DefaultValue = ""
            ATable.Columns("LocalCoordinate").DefaultValue = ""
            MydataSet.Tables("Ls_Common").Rows(0).Item("BB") = 1005

            'ATable = MyDataSet.Tables("LS_ComputingModelType")
            'ATable.Rows.Clear()
            'NewRow = ATable.Rows.Add
            'NewRow("ID") = 0
            'NewRow("Name") = "默认" '所有模式下的最小值
            'NewRow = ATable.Rows.Add
            'NewRow("ID") = 1
            'NewRow("Name") = "常规破坏" '常规承载力计算模式（均质土）
            'NewRow = ATable.Rows.Add
            'NewRow("ID") = 2
            'NewRow("Name") = "分层土破坏" '常规承载力计算模式（分层土）
            'NewRow = ATable.Rows.Add
            'NewRow("ID") = 3
            'NewRow("Name") = "挤出破坏"
            'NewRow = ATable.Rows.Add
            'NewRow("ID") = 4
            'NewRow("Name") = "穿刺破坏"
            'NewRow = ATable.Rows.Add
            'NewRow("ID") = 5
            'NewRow("Name") = "砂土拔桩力计算"
            'NewRow = ATable.Rows.Add
            'NewRow("ID") = 6
            'NewRow("Name") = "粘土拔桩力计算"

            'With MyDataSet.Tables("LS_CalculationLevels").Columns
            '    .Add("SelectMode_Qv", System.Type.GetType("System.Int32")) '承载力选择计算模式
            '    .Add("SelectMode_Qb", System.Type.GetType("System.Int32")) '拔桩力选择计算模式
            'End With
            'MyDataSet.Tables("LS_CalculationLevels").Columns("SelectMode_Qv").DefaultValue = 0
            'MyDataSet.Tables("LS_CalculationLevels").Columns("SelectMode_Qb").DefaultValue = 0

            'With MyDataSet.Tables("LS_PressResistanceResult").Columns
            '    .Add("SelectMode", System.Type.GetType("System.Int32")) '选择计算模式
            'End With
            'MyDataSet.Tables("LS_PressResistanceResult").Columns("SelectMode").DefaultValue = 0

            'With MyDataSet.Tables("LS_PullResistanceResult").Columns
            '    .Add("SelectMode", System.Type.GetType("System.Int32")) '选择计算模式
            'End With
            'MyDataSet.Tables("LS_PullResistanceResult").Columns("SelectMode").DefaultValue = 0

            'Dim DeepType As String() = {"无", "浅埋", "中等埋深", "深埋"}
            'For i = 1 To 2
            '    ATable = New DataTable("LS_TempDeepType" & i) '竖向地基承载力计算参数
            '    MyDataSet.Tables.Add(ATable)
            '    With ATable.Columns
            '        .Add("ID", System.Type.GetType("System.Int32"))
            '        .Add("Name", System.Type.GetType("System.String"))
            '    End With
            '    For j = 0 To 3
            '        NewRow = ATable.Rows.Add
            '        NewRow("ID") = j
            '        NewRow("Name") = DeepType(j)
            '    Next
            'Next

        End If
    End Sub
    Private Sub Update_1005()
        'Dim MyDataSet As DataSet = Application.GetStructureKit.GetData
        If Double.Parse(MydataSet.Tables("Ls_Common").Rows(0).Item("BB").ToString) = 1005 Then
            '***升级过程中的更新，后续添加到Update1003
            Dim ATable As DataTable = MydataSet.Tables("LS_CalculationAreas") '接触单元
            With ATable.Columns
                .Add("BeforeMaterialID", System.Type.GetType("System.Int32"))
            End With
            ATable.Columns("BeforeMaterialID").DefaultValue = 0

            ATable = MydataSet.Tables("LS_Contactors") '接触单元
            With ATable.Columns
                .Add("ID", System.Type.GetType("System.Int32")) 'ID
            End With
            MydataSet.Tables("Ls_Common").Rows(0).Item("BB") = 1006
        End If
    End Sub
    Private Sub Update_1006()
        'Dim MyDataSet As DataSet = Application.GetStructureKit.GetData
        If Double.Parse(MydataSet.Tables("Ls_Common").Rows(0).Item("BB").ToString) = 1006 Then
            '***升级过程中的更新，后续添加到Update1003
            Dim NewRow As DataRow
            Dim ATable As DataTable
            ATable = New DataTable("LS_StructureData") '竖向地基承载力计算参数
            With ATable.Columns
                .Add("WindFieldName", System.Type.GetType("System.String")) '风场名
                .Add("MachineLocationName", System.Type.GetType("System.String")) '机位名
                .Add("DrillName", System.Type.GetType("System.String")) '钻孔名
                .Add("BoatName", System.Type.GetType("System.String")) '船名
                .Add("UserName", System.Type.GetType("System.String")) '计算人姓名
                .Add("ContactNumber", System.Type.GetType("System.Double")) '联系方式
            End With
            MydataSet.Tables.Add(ATable)
            NewRow = ATable.Rows.Add
            NewRow("WindFieldName") = "风场1"
            NewRow("MachineLocationName") = "机位1"
            NewRow("DrillName") = "钻孔1"
            NewRow("BoatName") = "船1"
            NewRow("UserName") = "计算人甲"
            NewRow("ContactNumber") = 12345678900

            MydataSet.Tables.Remove("LS_ComputingModelType")
            ATable = New DataTable("LS_ComputingModelType_Qv")
            MydataSet.Tables.Add(ATable)
            With ATable.Columns
                .Add("ID", System.Type.GetType("System.Int32"))
                .Add("Name", System.Type.GetType("System.String"))
            End With
            NewRow = ATable.Rows.Add
            NewRow("ID") = 0
            NewRow("Name") = "默认" '所有模式下的最小值
            NewRow = ATable.Rows.Add
            NewRow("ID") = 1
            NewRow("Name") = "常规破坏" '常规承载力计算模式（均质土）
            NewRow = ATable.Rows.Add
            NewRow("ID") = 2
            NewRow("Name") = "分层土破坏" '常规承载力计算模式（分层土）
            NewRow = ATable.Rows.Add
            NewRow("ID") = 3
            NewRow("Name") = "挤出破坏"
            NewRow = ATable.Rows.Add
            NewRow("ID") = 4
            NewRow("Name") = "穿刺破坏"
            'NewRow = ATable.Rows.Add
            'NewRow("ID") = 5
            'NewRow("Name") = "砂土拔桩力计算"
            'NewRow = ATable.Rows.Add
            'NewRow("ID") = 6
            'NewRow("Name") = "粘土拔桩力计算"
            ATable = New DataTable("LS_ComputingModelType_Qb")
            MydataSet.Tables.Add(ATable)
            With ATable.Columns
                .Add("ID", System.Type.GetType("System.Int32"))
                .Add("Name", System.Type.GetType("System.String"))
            End With
            NewRow = ATable.Rows.Add
            NewRow("ID") = 0
            NewRow("Name") = "默认" '所有模式下的最小值
            NewRow = ATable.Rows.Add
            NewRow("ID") = 1
            NewRow("Name") = "砂土拔桩力计算"
            NewRow = ATable.Rows.Add
            NewRow("ID") = 2
            NewRow("Name") = "粘土拔桩力计算"
            With MydataSet.Tables("LS_CalculationLevels").Columns
                .Add("SelectMode_Qv", System.Type.GetType("System.Int32")) '承载力选择计算模式
                .Add("SelectMode_Qb", System.Type.GetType("System.Int32")) '拔桩力选择计算模式
            End With
            MydataSet.Tables("LS_CalculationLevels").Columns("SelectMode_Qv").DefaultValue = 0
            MydataSet.Tables("LS_CalculationLevels").Columns("SelectMode_Qb").DefaultValue = 0

            With MydataSet.Tables("LS_PressResistanceResult").Columns
                .Add("SelectMode", System.Type.GetType("System.Int32")) '选择计算模式
                .Add("Description", System.Type.GetType("System.String")) '公式描述
            End With
            MydataSet.Tables("LS_PressResistanceResult").Columns("SelectMode").DefaultValue = 0
            MydataSet.Tables("LS_PressResistanceResult").Columns("Description").DefaultValue = ""

            With MydataSet.Tables("LS_PullResistanceResult").Columns
                .Add("SelectMode", System.Type.GetType("System.Int32")) '选择计算模式
                .Add("Description", System.Type.GetType("System.String")) '公式描述
            End With
            MydataSet.Tables("LS_PullResistanceResult").Columns("SelectMode").DefaultValue = 0
            MydataSet.Tables("LS_PullResistanceResult").Columns("Description").DefaultValue = ""


            For Each Row In MydataSet.Tables("LS_PressResistanceResult").Rows
                Row("SelectMode") = 0
                Row("Description") = ""
            Next
            For Each Row In MydataSet.Tables("LS_PullResistanceResult").Rows
                Row("SelectMode") = 0
                Row("Description") = ""
            Next
            For Each Row In MydataSet.Tables("LS_CalculationLevels").Rows
                Row("SelectMode_Qv") = 0
                Row("SelectMode_Qb") = 0
            Next

            Dim DeepType As String() = {"无", "浅埋", "中等埋深", "深埋"}
            For i = 1 To 2
                ATable = New DataTable("LS_TempDeepType" & i) '竖向地基承载力计算参数
                MydataSet.Tables.Add(ATable)
                With ATable.Columns
                    .Add("ID", System.Type.GetType("System.Int32"))
                    .Add("Name", System.Type.GetType("System.String"))
                End With
                For j = 0 To 3
                    NewRow = ATable.Rows.Add
                    NewRow("ID") = j
                    NewRow("Name") = DeepType(j)
                Next
            Next
            MydataSet.Tables("Ls_Common").Rows(0).Item("BB") = 1007
        End If
    End Sub
    Private Sub Update_1007()
        'Dim MyDataSet As DataSet = Application.GetStructureKit.GetData
        If Double.Parse(MydataSet.Tables("Ls_Common").Rows(0).Item("BB").ToString) = 1007 Then
            Dim ATable As DataTable = MydataSet.Tables("LS_CalculationEdges")
            With ATable.Columns
                .Add("ReleaseID", System.Type.GetType("System.Int32"))
            End With
            ATable.Columns("ReleaseID").DefaultValue = 0
            For Each row In ATable.Rows
                row("ReleaseID") = 0
            Next


            ATable = New DataTable("LS_CoupleNodes") '耦合节点
            With ATable.Columns
                .Add("LevelID", System.Type.GetType("System.Int32")) '计算高程ID
                .Add("CoupleNodeID", System.Type.GetType("System.Int32")) '节点ID
                .Add("N1", System.Type.GetType("System.Int32")) '节点ID
                .Add("N2", System.Type.GetType("System.Int32")) '节点ID
                .Add("CoupleID", System.Type.GetType("System.Int32")) '节点ID
            End With
            MydataSet.Tables.Add(ATable)
            ATable.Columns("LevelID").DefaultValue = 0
            ATable.Columns("CoupleNodeID").DefaultValue = 0
            ATable.Columns("N1").DefaultValue = 0
            ATable.Columns("N2").DefaultValue = 0
            ATable.Columns("CoupleID").DefaultValue = 0

            ATable = MydataSet.Tables("Ls_Common")
            Dim UseSoileDrilling As Boolean = ATable.Rows(0)("UseSoilDrilling")
            ATable.Columns.Remove("UseSoilDrilling")
            With ATable.Columns
                .Add("UseSingleDrilling", System.Type.GetType("System.Boolean")) '单/多孔计算模式
                .Add("UseSoilDrilling", System.Type.GetType("System.Boolean"))
            End With
            ATable.Rows(0)("UseSingleDrilling") = True
            ATable.Rows(0)("UseSoilDrilling") = UseSoileDrilling
            '多钻孔计算模式
            ATable = MydataSet.Tables("LS_SoilDrilling")
            With ATable.Columns
                .Add("Name", System.Type.GetType("System.String")) '钻孔名称
            End With
            ATable.Columns("Name").DefaultValue = ""
            For Each row In ATable.Rows
                row("Name") = "钻孔#" & row("ID")
            Next
            '单钻孔计算模式
            ATable = MydataSet.Tables("LS_LegSoilLayer")
            With ATable.Columns
                .Add("DrillingID", System.Type.GetType("System.Int32")) '钻孔号
                .Add("DrillingName", System.Type.GetType("System.String")) '钻孔名称
            End With
            ATable.Columns("DrillingID").DefaultValue = 1
            ATable.Columns("DrillingName").DefaultValue = "钻孔#1"
            ATable.Columns("SoilID").DefaultValue = 0
            ATable.Columns("TopLevel").DefaultValue = 0
            For Each row In ATable.Rows
                row("DrillingID") = 1
                row("DrillingName") = "钻孔#1"
            Next
            Dim Tabs() As String = {"LS_CalculationLevels", "LS_Holl", "LS_PressResistanceResult", "LS_PullResistanceResult"}
            For Each ATab In Tabs
                ATable = MydataSet.Tables(ATab)
                With ATable.Columns
                    .Add("DrillingID", System.Type.GetType("System.Int32")) '钻孔号
                End With
                ATable.Columns("DrillingID").DefaultValue = 1
                For Each row In ATable.Rows
                    row("DrillingID") = 1
                Next
            Next
            MydataSet.Tables("Ls_Common").Rows(0).Item("BB") = 1008
        End If
    End Sub
    Private Sub Update_1008()
        'Dim MyDataSet As DataSet = Application.GetStructureKit.GetData
        If Double.Parse(MydataSet.Tables("Ls_Common").Rows(0).Item("BB").ToString) = 1008 Then
            Dim ATable As DataTable = MydataSet.Tables("Ls_Common")
            ATable.Rows(0)("SuInputType") = 1 '删除表格法输入抗剪强度Su，后续将表格法转变成线性法
            ATable = MydataSet.Tables("LS_StructureData")
            With ATable.Columns
                .Add("PullingCapacity", System.Type.GetType("System.Int32")) '拔桩能力(t)-船舶的参数
                .Add("WindFieldWaterHeight", System.Type.GetType("System.Double")) '风场区域水深(m)（17-19）
                .Add("AirGap", System.Type.GetType("System.Double")) '气隙(m)（5）
                .Add("GetJettingSystem", System.Type.GetType("System.Boolean")) '冲桩系统是否具备
                .Add("GoodWorking", System.Type.GetType("System.Boolean")) '工作状态是否良好
            End With
            For Each row In ATable.Rows
                row("PullingCapacity") = 3500
                row("WindFieldWaterHeight") = 17 '17-19
                row("AirGap") = 5
                row("GetJettingSystem") = True
                row("GoodWorking") = True
            Next
            ATable = MydataSet.Tables("LS_Leg")
            With ATable.Columns
                .Add("ActiveLength", System.Type.GetType("System.Double")) '有效长度(m)
            End With
            For Each row In ATable.Rows
                row("ActiveLength") = 50
            Next
            ATable = MydataSet.Tables("LS_Soil")
            ATable.Columns("UnderWaterWeight").DefaultValue = 8
            For Each row In ATable.Rows
                Dim Weight As Double = row("UnderWaterWeight") - 10
                row("UnderWaterWeight") = Double.Parse(Weight.ToString("N6")) '饱和重度更改为浮重度
            Next
            ATable = MydataSet.Tables("LS_CalculationParameter")
            Dim CalculationMethod As Integer = ATable.Rows(0)("CalculatieMethod")
            ATable.Columns.Remove("CalculatieMethod")
            With ATable.Columns
                .Add("PressForce", System.Type.GetType("System.Int32")) '单腿预压力(t)
                .Add("CalculationMethod", System.Type.GetType("System.Int32")) '计算方法，公式法还是有限元法
            End With
            For Each row In ATable.Rows
                row("PressForce") = 4500
                row("CalculationMethod") = CalculationMethod
            Next
            '计算结果简表
            ATable = New DataTable("LS_DepthResult")
            MydataSet.Tables.Add(ATable)
            With ATable.Columns
                .Add("LimitForce", System.Type.GetType("System.Int32")) '测试力(t) 
                .Add("DrillingID", System.Type.GetType("System.Int32")) '钻孔号
                .Add("SuggestedDepth", System.Type.GetType("System.Double")) '建议插深
                .Add("SupportSoilID", System.Type.GetType("System.Int32")) '持力层土
                .Add("SupportSoilStrength", System.Type.GetType("System.Double")) '持力层土强度参数(黏土是抗剪强度，砂土是内摩擦角)
                .Add("SelectMode_Qv", System.Type.GetType("System.Int32")) '抗压承载力计算模式
                .Add("Qv", System.Type.GetType("System.String")) '地基承载力(kN)
                .Add("Qu", System.Type.GetType("System.String")) '抗拔力(kN)
                .Add("IsUserAdd", System.Type.GetType("System.Boolean")) 'True-写入结论，False-写入结果表
            End With
            For Each AColumn As DataColumn In ATable.Columns
                If AColumn.DataType = Type.GetType("System.Double") Or AColumn.DataType = Type.GetType("System.Int32") Or AColumn.DataType = Type.GetType("System.Boolean") Then
                    AColumn.DefaultValue = 0
                ElseIf AColumn.DataType = Type.GetType("System.String") Then
                    AColumn.DefaultValue = ""
                End If
            Next
            '修改列数据类型
            '.Add("QvP", System.Type.GetType("System.String")) '选择计算模式下的地基承载力(kPa)
            '.Add("Qv", System.Type.GetType("System.String")) '选择计算模式下的地基承载力(kN)
            '.Add("Qv1", System.Type.GetType("System.String")) '常规破坏(暂不使用)
            '.Add("Qv1_Sand", System.Type.GetType("System.String")) '常规破坏(砂土)
            '.Add("Qv1_Clay", System.Type.GetType("System.String")) '常规破坏(黏土)
            '.Add("Qv2", System.Type.GetType("System.String")) '挤出破坏结果
            '.Add("Qv3", System.Type.GetType("System.String")) '穿刺破坏(暂不使用)
            '.Add("Qv3_Sand", System.Type.GetType("System.String")) '穿刺破坏(砂土穿刺黏土)
            '.Add("Qv3_Clay", System.Type.GetType("System.String")) '穿刺破坏(黏土穿刺黏土)
            '.Add("Qv4", System.Type.GetType("System.String")) '分层土破坏
            Dim ResultDic As New Dictionary(Of Integer, Dictionary(Of Double, List(Of Double)))
            Dim ResultTitle As String() = {"QvP", "Qv", "Qv1", "Qv1_Sand", "Qv1_Clay", "Qv2", "Qv3", "Qv3_Sand", "Qv3_Clay", "Qv4"}
            ATable = MydataSet.Tables("LS_PressResistanceResult")
            For Each row In ATable.Select("", "DrillingID ASC,Level DESC")
                If ResultDic.ContainsKey(row("DrillingID")) = False Then
                    ResultDic.Add(row("DrillingID"), New Dictionary(Of Double, List(Of Double)))
                End If
                If ResultDic(row("DrillingID")).ContainsKey(row("Level")) = False Then
                    ResultDic(row("DrillingID")).Add(row("Level"), New List(Of Double))
                End If
                For Each QvTitle In ResultTitle
                    ResultDic(row("DrillingID"))(row("Level")).Add(row(QvTitle))
                Next
            Next
            For Each Title In ResultTitle
                ATable.Columns.Remove(Title)
                ATable.Columns.Add(Title, System.Type.GetType("System.String"))
                ATable.Columns(Title).DefaultValue = ""
            Next
            For Each row In ATable.Rows
                For i = 0 To ResultTitle.Count - 1
                    row(ResultTitle(i)) = If(ResultDic(row("DrillingID"))(row("Level"))(i) = 10 ^ 10, "-", ResultDic(row("DrillingID"))(row("Level"))(i))
                Next
            Next
            '.Add("QuP", System.Type.GetType("System.String")) '抗拔力(t)
            '.Add("Qu", System.Type.GetType("System.String")) '抗拔力(kN)
            '.Add("Qu_Sand", System.Type.GetType("System.String")) '抗拔力
            '.Add("Qu_Clay", System.Type.GetType("System.String")) '抗拔力
            ResultDic = New Dictionary(Of Integer, Dictionary(Of Double, List(Of Double)))
            ResultTitle = {"QuP", "Qu", "Qu_Sand", "Qu_Clay"}
            ATable = MydataSet.Tables("LS_PullResistanceResult")
            For Each row In ATable.Select("", "DrillingID ASC,Level DESC")
                If ResultDic.ContainsKey(row("DrillingID")) = False Then
                    ResultDic.Add(row("DrillingID"), New Dictionary(Of Double, List(Of Double)))
                End If
                If ResultDic(row("DrillingID")).ContainsKey(row("Level")) = False Then
                    ResultDic(row("DrillingID")).Add(row("Level"), New List(Of Double))
                End If
                For Each QvTitle In ResultTitle
                    ResultDic(row("DrillingID"))(row("Level")).Add(row(QvTitle))
                Next
            Next
            For Each Title In ResultTitle
                ATable.Columns.Remove(Title)
                ATable.Columns.Add(Title, System.Type.GetType("System.String"))
                ATable.Columns(Title).DefaultValue = ""
            Next
            For Each row In ATable.Rows
                For i = 0 To ResultTitle.Count - 1
                    row(ResultTitle(i)) = If(ResultDic(row("DrillingID"))(row("Level"))(i) = 10 ^ 10, "-", ResultDic(row("DrillingID"))(row("Level"))(i))
                Next
            Next
            MydataSet.Tables("Ls_Common").Rows(0).Item("BB") = 1009
        End If
    End Sub
    Private Sub Update_1009()
        'Dim MyDataSet As DataSet = Application.GetStructureKit.GetData
        If Double.Parse(MydataSet.Tables("Ls_Common").Rows(0).Item("BB").ToString) = 1009 Then
            Dim ATable As DataTable
            ATable = MydataSet.Tables("LS_SoilType")
            ATable.Select("ID=0")(0)("Name") = "黏土"

            ATable = MydataSet.Tables("LS_CalculationParameter")
            ATable.Columns.Add("fb", System.Type.GetType("System.Double")) '冲桩减阻系数fb
            ATable.Columns("CalculationMethod").DefaultValue = 1
            ATable.Columns("DestinationLevel").DefaultValue = -20
            ATable.Columns("NCalculatePoint").DefaultValue = 20
            ATable.Columns("PressForce").DefaultValue = 4500
            ATable.Columns("MeshSize").DefaultValue = 1
            ATable.Columns("DPType").DefaultValue = 4
            ATable.Columns("KeepHistory").DefaultValue = 1 '设置默认值
            ATable.Columns("DCoeff").DefaultValue = 0.001
            ATable.Columns("IsBackFlow").DefaultValue = 1
            ATable.Columns("AutoGetHc").DefaultValue = True
            ATable.Columns("Hc").DefaultValue = 0
            ATable.Columns("ftop").DefaultValue = 1
            ATable.Columns("fbase").DefaultValue = 1
            ATable.Columns("NBreakout").DefaultValue = 1
            ATable.Columns("SoilCoarseCoeff").DefaultValue = 0.5
            ATable.Columns("fb").DefaultValue = 0
            'Dim PressForce As Double = ATable.Rows(0)("PressForce")
            'ATable.Columns.Remove("PressForce")
            ATable.Rows(0)("fb") = 0
            'ATable.Rows(0)("PressForce") = PressForce

            For Each TabName In {"LS_PullResistanceResult", "LS_DepthResult"}
                ATable = MydataSet.Tables(TabName)
                ATable.Columns.Add("Qu0", System.Type.GetType("System.String")) '冲桩减阻系数fb=0时抗拔力(kN)
                ATable.Columns.Add("Qu1", System.Type.GetType("System.String")) '冲桩减阻系数fb=1时抗拔力(kN)
                ATable.Columns("Qu0").DefaultValue = ""
                ATable.Columns("Qu1").DefaultValue = ""
                For Each row In ATable.Rows
                    row("Qu0") = ""
                    row("Qu1") = ""
                Next
            Next
            ATable = MydataSet.Tables("LS_DepthResult")
            Dim LimitValues As New List(Of Double)
            For Each Row In ATable.Select("", "DrillingID")
                LimitValues.Add(Row("LimitForce"))
            Next
            ATable.Columns.Remove("LimitForce")
            ATable.Columns.Add("LimitForce", System.Type.GetType("System.Double")) '测试力(t)
            For i = 0 To ATable.Select("", "DrillingID").Count - 1
                ATable.Select("", "DrillingID")(i)("LimitForce") = LimitValues(i)
            Next
            ATable = MydataSet.Tables("LS_StructureData")
            Dim PullingCapacity As Double = ATable.Rows(0)("PullingCapacity")
            ATable.Columns.Remove("MachineLocationName")
            ATable.Columns.Remove("DrillName")
            ATable.Columns.Remove("PullingCapacity")
            ATable.Columns.Add("PullingCapacity", System.Type.GetType("System.Double")) '拔桩能力(t)-船舶的参数
            ATable.Rows(0)("PullingCapacity") = PullingCapacity

            ATable = MydataSet.Tables("LS_SpudcanType")
            ATable.Clear()
            Dim NewRow As DataRow
            NewRow = ATable.Rows.Add
            NewRow("ID") = 0
            NewRow("Name") = "类圆形"
            NewRow = ATable.Rows.Add
            NewRow("ID") = 1
            NewRow("Name") = "类四边形"
            MydataSet.Tables("Ls_Common").Rows(0).Item("BB") = 1010
        End If
    End Sub
    Private Sub Update_10000(Optional ByVal Boats As Boolean = True)
        If Boats And Double.Parse(MydataSet.Tables("Ls_Common").Rows(0).Item("BB").ToString) = 10000 Then
            Dim ATable As DataTable
            ATable = MydataSet.Tables("LS_Boat")
            ATable.Columns.Add("LegA", System.Type.GetType("System.Double")) '桩腿面积(m2) 
            ATable.Columns("LegA").DefaultValue = 0
            For Each row In ATable.Rows
                row("LegA") = row("LegDiameter") ^ 2 * PI / 4 'BB=10000时内部默认值，现开放用户可编辑
            Next
            MydataSet.Tables("Ls_Common").Rows(0).Item("BB") = 10001
        End If
    End Sub

    Private Sub Update_1010(Boats As Boolean)
        If MydataSet.Tables("Ls_Common").Rows(0).Item(If(Boats, "ABoatBB", "BB")) = 1010 Then
            Dim ATable As DataTable
            ATable = MydataSet.Tables("LS_CalculationParameter")
            ATable.Columns.Add("fleg", System.Type.GetType("System.Double")) '桩腿侧摩阻力系数fleg，取值范围0~1，默认取0
            ATable.Columns("NCalculatePoint").DefaultValue = 1 '默认为1，即计算每个地层分界面位置
            ATable.Columns("AutoGetHc").DefaultValue = False '默认按全回流高度计算，Hc=0
            ATable.Columns("NBreakout").DefaultValue = 8
            ATable.Columns("fleg").DefaultValue = 0
            For Each row In ATable.Rows
                row("fleg") = 0
            Next
            MydataSet.Tables("Ls_Common").Rows(0).Item(If(Boats, "ABoatBB", "BB")) = 1011
        End If
    End Sub
    Private Sub Update_1011(Boats As Boolean)
        If MydataSet.Tables("Ls_Common").Rows(0).Item(If(Boats, "ABoatBB", "BB")) = 1011 Then
            Dim ATable As DataTable
            ATable = MydataSet.Tables("LS_CalculationParameter")
            ATable.Columns("fb").DefaultValue = 1 '
            ATable.Columns("fleg").DefaultValue = 0.5
            MydataSet.Tables("Ls_Common").Rows(0).Item(If(Boats, "ABoatBB", "BB")) = 1012
        End If
    End Sub
    Private Sub Update_1012(Boats As Boolean)
        If MydataSet.Tables("Ls_Common").Rows(0).Item(If(Boats, "ABoatBB", "BB")) = 1012 Then
            If Not Boats Then
                Dim ATable As DataTable
                ATable = MydataSet.Tables("LS_Soil")
                ATable.Columns.Add("DrillingID", System.Type.GetType("System.Int32")) '钻孔号
                ATable.Columns("DrillingID").DefaultValue = 1
                For Each row In ATable.Rows
                    row("DrillingID") = 1
                Next
                '单船计算时，多钻孔共用土层参数表，多船计算时，多钻孔不共用土层参数表，一钻孔对应多土层参数，添加DrillingID加以区分
            End If
            MydataSet.Tables("Ls_Common").Rows(0).Item(If(Boats, "ABoatBB", "BB")) = 1013
        End If
    End Sub
    Private Sub Update_1013(Boats As Boolean)
        If MydataSet.Tables("Ls_Common").Rows(0).Item(If(Boats, "ABoatBB", "BB")) = 1013 Then
            Dim ATable As DataTable
            ATable = MydataSet.Tables("LS_CalculationParameter")
            ATable.Columns.Add("IsEquivalentToCircleSpudcan", System.Type.GetType("System.Boolean")) '(砂土)是否等效为圆形桩靴
            ATable.Columns("IsEquivalentToCircleSpudcan").DefaultValue = True
            For Each row In ATable.Rows
                row("IsEquivalentToCircleSpudcan") = True
            Next
            If Not Boats Then
                ATable = MydataSet.Tables("LS_Spudcan")
                ATable.Columns.Remove("IsCoarse")
            End If
            MydataSet.Tables("Ls_Common").Rows(0).Item(If(Boats, "ABoatBB", "BB")) = 1014
        End If
    End Sub
    Private Sub Update_10001(Boats As Boolean)
        If Boats And Double.Parse(MydataSet.Tables("Ls_Common").Rows(0).Item("BB").ToString) = 10001 Then
            Dim ATable As DataTable
            ATable = MydataSet.Tables("LS_Boat")
            ATable.Columns.Remove("IsSpudcanCoarse")
            MydataSet.Tables("Ls_Common").Rows(0).Item("BB") = 10002
        End If
    End Sub
    Private Sub Update_10002(Boats As Boolean)
        If Boats And Double.Parse(MydataSet.Tables("Ls_Common").Rows(0).Item("BB").ToString) = 10002 Then
            Dim ATable As DataTable
            ATable = MydataSet.Tables("LS_Boat")
            Dim LDic As New Dictionary(Of Integer, Double)
            For Each row In ATable.Rows
                LDic.Add(row("ID"), row("SpudcanL"))
            Next
            ATable.Columns.Remove("SpudcanL")
            ATable.Columns.Add("SpudcanL", System.Type.GetType("System.String")) '桩靴长度(m)
            For Each row In ATable.Rows
                row("SpudcanL") = LDic(row("ID")).ToString
            Next
            MydataSet.Tables("Ls_Common").Rows(0).Item("BB") = 10003
        End If
    End Sub
    Private Sub Update_1014(Boats As Boolean)
        If MydataSet.Tables("Ls_Common").Rows(0).Item(If(Boats, "ABoatBB", "BB")) = 1014 Then
            Dim ATable As DataTable
            ATable = MydataSet.Tables("LS_CalculationParameter")
            ATable.Columns.Add("UnderWaterPhiSubtractValue", System.Type.GetType("System.Double")) '砂土内摩擦角降低度数
            ATable.Columns("UnderWaterPhiSubtractValue").DefaultValue = 5
            For Each row In ATable.Rows
                row("UnderWaterPhiSubtractValue") = 5
            Next
            MydataSet.Tables("Ls_Common").Rows(0).Item(If(Boats, "ABoatBB", "BB")) = 1015
        End If
    End Sub
    Private Sub Update_1015(Boats As Boolean) '20230828
        If MydataSet.Tables("Ls_Common").Rows(0).Item(If(Boats, "ABoatBB", "BB")) = 1015 Then
            Dim ATable As DataTable
            ATable = MydataSet.Tables("LS_CalculationParameter")
            ATable.Columns.Add("IsSealed", System.Type.GetType("System.Boolean")) '桩靴是否密封扣除浮力
            ATable.Columns.Add("GroundPressure", System.Type.GetType("System.Double")) '对地比压(kPa)，同LS_Boat中的GroundPressure，用来画图
            ATable.Columns("IsSealed").DefaultValue = True
            ATable.Columns("GroundPressure").DefaultValue = 233.64
            ATable.Columns("fb").DefaultValue = 0
            For Each row In ATable.Rows
                row("IsSealed") = True
                row("GroundPressure") = If(Boats, MydataSet.Tables("LS_Boat").Select("ID=" & row("BoatID"))(0)("GroundPressure"), row("PressForce") * 9.8 / MydataSet.Tables("LS_Spudcan").Rows(0)("Area"))
            Next
            MydataSet.Tables("Ls_Common").Rows(0).Item(If(Boats, "ABoatBB", "BB")) = 1016
        End If
    End Sub
    Private Sub Update_10003(Boats As Boolean) '20231025
        If Boats And MydataSet.Tables("Ls_Common").Rows(0).Item("BB") = 10003 Then
            Dim ATable As DataTable
            ATable = MydataSet.Tables("LS_SoilDrillingParameter")
            Dim NListDic As New Dictionary(Of Integer, List(Of String))
            For Each row In ATable.Rows
                If Not NListDic.ContainsKey(row("BoatID")) Then
                    NListDic.Add(row("BoatID"), New List(Of String))
                End If
                NListDic(row("BoatID")).Add(row("DrillingID") & "," & row("ID") & "," & row("N"))
            Next
            ATable.Columns.Remove("N")
            ATable.Columns.Add("N", System.Type.GetType("System.Double")) '标贯击数 
            For Each row In ATable.Rows
                For Each SDP In NListDic(row("BoatID"))
                    Dim DID As Integer = Integer.Parse(SDP.Split(",")(0))
                    Dim SID As Integer = Integer.Parse(SDP.Split(",")(1))
                    Dim N As Double = Double.Parse(SDP.Split(",")(1))
                    If DID = row("DrillingID") And SID = row("ID") Then
                        row("N") = N
                        Exit For
                    End If
                Next
            Next
            MydataSet.Tables("Ls_Common").Rows(0).Item("BB") = 10004
        End If
    End Sub
    Sub Update_1016(Boats As Boolean)
        If MydataSet.Tables("Ls_Common").Rows(0).Item(If(Boats, "ABoatBB", "BB")) = 1016 Then
            If Not Boats Then
                Dim ATable As DataTable
                ATable = MydataSet.Tables("LS_Soil")
                With ATable.Columns
                    .Add("OnLegWeightReduceCoeff", System.Type.GetType("System.Double")) '折减系数
                    .Add("OnLegStrenthengReduceCoeff", System.Type.GetType("System.Double")) '折减系数
                    .Add("OnLegEReduceCoeff", System.Type.GetType("System.Double")) '折减系数
                    .Add("OnLegMuReduceCoeff", System.Type.GetType("System.Double")) '折减系数
                End With
                ATable.Columns("OnLegWeightReduceCoeff").DefaultValue = 1
                ATable.Columns("OnLegStrenthengReduceCoeff").DefaultValue = 1
                ATable.Columns("OnLegEReduceCoeff").DefaultValue = 1
                ATable.Columns("OnLegMuReduceCoeff").DefaultValue = 1
            End If
            MydataSet.Tables("Ls_Common").Rows(0).Item(If(Boats, "ABoatBB", "BB")) = 1017
        End If
    End Sub
    Private Sub Update_1017(Boats As Boolean) '20230828
        If MydataSet.Tables("Ls_Common").Rows(0).Item(If(Boats, "ABoatBB", "BB")) = 1017 Then
            Dim ATable As DataTable
            ATable = MydataSet.Tables("LS_CalculationParameter")
            ATable.Columns.Add("HC2", System.Type.GetType("System.Double")) '桩靴是否密封扣除浮力
            ATable.Columns.Add("cohesionCoeff", System.Type.GetType("System.Double")) '桩靴是否密封扣除浮力

            For Each row As DataRow In ATable.Rows
                row("HC2") = 0
                row("cohesionCoeff") = 0
            Next
            MydataSet.Tables("Ls_Common").Rows(0).Item(If(Boats, "ABoatBB", "BB")) = 1018
        End If
    End Sub
    Private Sub Update_1018(Boats As Boolean) '20230828
        If Double.Parse(MydataSet.Tables("Ls_Common").Rows(0).Item(If(Boats, "ABoatBB", "BB")).ToString) = 1018 Then
            Dim ATable As DataTable = MydataSet.Tables("LS_CalculationAreas")
            With ATable.Columns
                .Add("Location", System.Type.GetType("System.Int32")) '与桩靴位置关系，1在上面，2 在下面
            End With

            MydataSet.Tables("Ls_Common").Rows(0).Item(If(Boats, "ABoatBB", "BB")) = 1019
        End If
    End Sub
    Private Sub Update_1019(Boats As Boolean) '20250928
        If Double.Parse(MydataSet.Tables("Ls_Common").Rows(0).Item(If(Boats, "ABoatBB", "BB")).ToString) = 1019 Then
            Dim ATable As DataTable = New DataTable("LS_PunctureRiskAssessmentResult")
            MydataSet.Tables.Add(ATable)
            With ATable.Columns
                .Add("DrillingID", System.Type.GetType("System.Int32")) '钻孔号
                .Add("P1", System.Type.GetType("System.String")) '对地比压
                .Add("P2", System.Type.GetType("System.String")) '持力层土强度参数(黏土是抗剪强度，砂土是内摩擦角)
                .Add("P3", System.Type.GetType("System.String")) '地基承载力(kN)
                .Add("Fs1", System.Type.GetType("System.String")) '考虑冲桩减阻系数fb抗拔力(kN)
                .Add("Fs2", System.Type.GetType("System.String")) '冲桩减阻系数fb=0时抗拔力(kN)
                .Add("IsPunctureRiskOK", System.Type.GetType("System.Boolean")) '冲桩减阻系数fb=1时抗拔力(kN)
            End With
            For Each AColumn As DataColumn In ATable.Columns
                If AColumn.DataType = Type.GetType("System.Double") Or AColumn.DataType = Type.GetType("System.Int32") Or AColumn.DataType = Type.GetType("System.Boolean") Then
                    AColumn.DefaultValue = 0
                ElseIf AColumn.DataType = Type.GetType("System.String") Then
                    AColumn.DefaultValue = ""
                End If
            Next
            MydataSet.Tables("Ls_Common").Rows(0).Item(If(Boats, "ABoatBB", "BB")) = 1020
        End If
    End Sub
    Public Function GetSoilLayerMesh() As List(Of EsTLLayerMesh)
        '各个孔参数
        Dim SoilColumns As New List(Of EsTLSoilColumn)
        Dim LayerNames As New List(Of String)
        Dim Meshs As New List(Of EsTLLayerMesh)
        Dim SoilLayerCount As Integer = 0
        For Each row As DataRow In MydataSet.Tables("LS_SoilDrilling").Rows()
            Dim ASoilColumn As New EsTLSoilColumn
            SoilColumns.Add(ASoilColumn)
            ASoilColumn.Location.x = row("x")
            ASoilColumn.Location.y = row("y")
            Dim SoilLayers() As String = Split(row("SoilLayers"), ";")
            For Each SoilLayer In SoilLayers
                Dim SoilNameAndLevel() As String = Split(SoilLayer, ",")
                Dim Alayer As New EsTLSoilColumnLayer
                Alayer.Material.Name = SoilNameAndLevel(0)
                Alayer.z = Val(SoilNameAndLevel(1))
                ASoilColumn.Layers.Add(Alayer)
            Next
            SoilLayerCount = SoilLayers.Length
        Next
        Dim Parameter As New EsTLSoilDEMParameter
        Parameter.RegionType = EsSoilDEMRegionType.ByRegionPoint
        Dim Minx, Maxx, Miny, Maxy As Double
        For i As Integer = 0 To SoilColumns.Count - 1
            If i = 0 Then
                Minx = SoilColumns(i).Location.x
                Miny = SoilColumns(i).Location.y
                Maxx = SoilColumns(i).Location.x
                Maxy = SoilColumns(i).Location.y
            Else
                Minx = Min(Minx, SoilColumns(i).Location.x)
                Miny = Min(Miny, SoilColumns(i).Location.y)
                Maxx = Max(Maxy, SoilColumns(i).Location.x)
                Maxy = Max(Maxy, SoilColumns(i).Location.y)
            End If
        Next





        Parameter.RegionPoints.Add(New EsTLPoint2D(1, Minx, Miny))
        Parameter.RegionPoints.Add(New EsTLPoint2D(2, Maxx, Miny))
        Parameter.RegionPoints.Add(New EsTLPoint2D(3, Maxx, Maxy))
        Parameter.RegionPoints.Add(New EsTLPoint2D(4, Minx, Maxy))
        Parameter.Rx = Math.Sqrt((Maxx - Minx) ^ 2 + (Maxy - Miny) ^ 2) / 10
        Parameter.Ry = Parameter.Rx
        Parameter.MeshDLx = Parameter.Rx / 2
        Parameter.MeshDLy = Parameter.Ry / 2
        Meshs = EsTLSoilGeometryKit.GenSoilLayersDEM(SoilColumns, SoilLayerCount, Parameter)


        Return Meshs
    End Function
    Public Function GetSoils() As Dictionary(Of String, EsSoil)
        Dim Soils As New Dictionary(Of String, EsSoil)
        For Each row As DataRow In MydataSet.Tables("LS_Soil").Rows
            Dim Soil As New EsSoil
            Soil.ID = row("ID")
            Soil.Name = row("Name")
            Soils.Add(Soil.Name, Soil)
        Next
        Return Soils
    End Function
    Private Shared Function GetDrillingName(ByVal MyDataSet As DataSet, DrillingID As Integer, BoatID As Integer) As String
        Dim DrillingName As String = ""
        If MyDataSet.Tables("LS_Common").Columns.Contains("UseSingleDrilling") Then
            Dim UseSingleDrilling As Boolean = MyDataSet.Tables("LS_Common").Rows(0)("UseSingleDrilling")
            If Not UseSingleDrilling Then
                DrillingName = MyDataSet.Tables("LS_SoilDrilling").Select("ID=" & DrillingID, "ID")(0)("Name")
            End If
        Else
            DrillingName = MyDataSet.Tables("LS_SoilDrillingParameter").Select("DrillingID=" & DrillingID & If(BoatID = -1, "", " and BoatID=" & BoatID))(0)("DrillingName")
        End If
        Dim DrillingNameSuffixes As String() = {"最小", "中值", "最大"}
        For i = 0 To DrillingNameSuffixes.Length - 1
            If DrillingName.EndsWith("-" & DrillingNameSuffixes(i)) Then
                DrillingName = DrillingName.Remove(DrillingName.Length - 3, 3)
                Exit For
            End If
        Next
        Return DrillingName
    End Function
    Friend Shared Function GetDrillingIDs(ByVal MyDataSet As DataSet, DrillingID As Integer, BoatID As Integer) As List(Of Integer)
        Dim DrillingIDs As New List(Of Integer)
        Dim DrillingName As String = GetDrillingName(MyDataSet, DrillingID, BoatID)
        Dim DrillingNameSuffixes As String() = {"最小", "中值", "最大"}
        If Not MyDataSet.Tables("LS_Common").Columns.Contains("UseSingleDrilling") Then
            For i = 0 To DrillingNameSuffixes.Length - 1
                For Each row In MyDataSet.Tables("LS_SoilDrillingParameter").Select("DrillingName='" & DrillingName & "-" & DrillingNameSuffixes(i) & "'" & If(BoatID = -1, "", " and BoatID=" & BoatID))
                    If Not DrillingIDs.Contains(Integer.Parse(row("DrillingID").ToString)) Then
                        DrillingIDs.Add(Integer.Parse(row("DrillingID").ToString))
                    End If
                Next
            Next
        End If
        If DrillingIDs.Count = 0 Then
            DrillingIDs.Add(DrillingID)
        End If
        Return DrillingIDs
    End Function
    Private Shared Function GetLayerLevels(ByVal MyDataSet As DataSet, DrillingID As Integer, BoatID As Integer) As List(Of Double)
        Dim MustShowV2s As New List(Of Double)
        If MyDataSet.Tables("LS_Common").Columns.Contains("UseSingleDrilling") Then
            Dim UseSingleDrilling As Boolean = MyDataSet.Tables("LS_Common").Rows(0)("UseSingleDrilling")
            If UseSingleDrilling Then
                For Each row As DataRow In MyDataSet.Tables("LS_LegSoilLayer").Select
                    If Not MustShowV2s.Contains(row("TopLevel")) Then MustShowV2s.Add(row("TopLevel"))
                Next
            Else
                For Each row As DataRow In MyDataSet.Tables("LS_SoilDrilling").Select("ID=" & DrillingID, "ID")
                    Dim SoilLayers() As String = Split(row("SoilLayers"), ";")
                    For Each Layer In SoilLayers
                        If Not MustShowV2s.Contains(Val(Layer.Split(",")(1))) Then MustShowV2s.Add(Val(Layer.Split(",")(1)))
                    Next
                Next
            End If
        Else
            For Each row As DataRow In MyDataSet.Tables("LS_SoilDrillingParameter").Select("DrillingID=" & DrillingID & If(BoatID = -1, "", " and BoatID=" & BoatID))
                If Not MustShowV2s.Contains(row("TopLevel")) Then MustShowV2s.Add(row("TopLevel"))
                If Not MustShowV2s.Contains(row("TipLevel")) Then MustShowV2s.Add(row("TipLevel"))
            Next
        End If
        Return MustShowV2s
    End Function
    'Shared Function DrawPressCurve(MyDataSet As DataSet, PressCurveTable As EsPLCurveTable, LimitValue As Double, Width As Double, Height As Double, Optional XSep As Double = 1, Optional YSep As Double = 1, Optional DrillingID As Integer = 1, Optional BoatID As Integer = -1, Optional UseMetaFile As Boolean = True, Optional ResultRangeMultiple As Double = 2, Optional LimitValueShowMultiple As Double() = Nothing) As Drawing.Image '.NET8.0不兼容System.Drawing.Image，需替代
    '    On Error Resume Next
    '    If IsNothing(LimitValueShowMultiple) Then
    '        LimitValueShowMultiple = {1, 1.2, 1.5}
    '    End If
    '    'Dim SpudcanArea As Double = If(MyDataSet.Tables("LS_Common").Columns.Contains("UseSingleDrilling"), MyDataSet.Tables("LS_Spudcan").Rows(0)("Area"), MyDataSet.Tables("LS_Boat").Rows(0)("SpudcanA"))
    '    Dim DrillingName As String = GetDrillingName(MyDataSet, DrillingID, BoatID)
    '    ''压弯曲线
    '    PressCurveTable.ScaleFontSize = 10
    '    PressCurveTable.ScaleFontColor = New EsPLColor(0, 176 / 255, 240 / 255) 'WPS文字浅蓝
    '    PressCurveTable.ScaleHorizontalAlternateShow = True
    '    PressCurveTable.ScaleVerticalAlternateShow = False
    '    PressCurveTable.VerticalTitle.FontSize = 12
    '    PressCurveTable.VerticalTitle.Text = "插深(m)"
    '    PressCurveTable.VerticalTitle2 = "桩靴底标高(m)"
    '    PressCurveTable.VerticalTitle.Alignment = EsPLTextAlignment.Center
    '    PressCurveTable.VerticalTitle.LineAlignment = EsPLTextAlignment.Right
    '    PressCurveTable.VerticalTitle.Angle = -90
    '    PressCurveTable.VerticalTitleFormat2 = "0.00"

    '    PressCurveTable.HorizontalTitle.FontSize = 12
    '    PressCurveTable.HorizontalTitle.Text = DrillingName & If(DrillingName = "", "", "-") & "地基承载力Qv(kPa)"
    '    Dim BoatName As String = If(BoatID = -1, "", MyDataSet.Tables("LS_Boat").Select("ID=" & BoatID)(0)("Name").ToString)
    '    PressCurveTable.HorizontalTitle2 = DrillingName & If(DrillingName = "", "", "-") & "地基承载力Qv(kN)" & vbCrLf & BoatName '20250929图名中在最下方加入计算的平台船名称
    '    PressCurveTable.HorizontalTitle.Alignment = EsPLTextAlignment.Center
    '    PressCurveTable.HorizontalTitle.LineAlignment = EsPLTextAlignment.Left
    '    PressCurveTable.HorizontalTitleFormat2 = "0"

    '    PressCurveTable.WithGrid = True
    '    PressCurveTable.GridLineStyle = EsPLLineStyle.Solid
    '    PressCurveTable.GridLineColor = New EsPLColor(0.918, 0.918, 0.918) '灰色
    '    PressCurveTable.LegendLocation = EsPLLegendLocation.LeftBottom
    '    PressCurveTable.Curves.Clear()

    '    Dim DrillingIDs As List(Of Integer) = GetDrillingIDs(MyDataSet, DrillingID, BoatID)

    '    Dim SelecDIDString As String = ""
    '    For i = 0 To DrillingIDs.Count - 1
    '        SelecDIDString = "DrillingID= " & DrillingIDs(i) & If(i = DrillingIDs.Count - 1, "", " or ")
    '    Next
    '    Dim TopLevel As Double = MyDataSet.Tables("LS_CalculationLevels").Compute("Max(Level)", SelecDIDString & If(BoatID = -1, "", " and BoatID=" & BoatID))

    '    Dim Curve As EsPLCurve
    '    Dim CurveLC As EsPLColor() = {New EsPLColor(0, 0, 0), New EsPLColor(237 / 255, 125 / 255, 49 / 255), New EsPLColor(155 / 255, 187 / 255, 89 / 255)} '黑橙绿
    '    For IDi = 0 To DrillingIDs.Count - 1

    '        Curve = New EsPLCurve
    '        Curve.Color = CurveLC(IDi)
    '        Curve.LineWidth = 2
    '        Curve.MarkType = ESPLMarkType.Point
    '        Curve.MarkColor = If(DrillingIDs.Count = 1, CurveLC(2), CurveLC(IDi))
    '        Curve.ShowPointText = True
    '        Curve.ShowCurveValueStyle = EsPLShowCurveValueStyle.XValue
    '        Curve.PointFontSize = 12
    '        'Dim TopLevel As Double = MyDataSet.Tables("LS_LegSoilLayer").Compute("Max(TopLevel)", "")
    '        'For Each row As DataRow In MyDataSet.Tables("LS_PressResistanceResult").Select("DrillingID=" & DrillingID & " And QvP<>'-'" & If(BoatID = -1, "", " and BoatID=" & BoatID), "Level DESC")
    '        '    'If Round(LimitValue / SpudcanArea * 2, 2) >= Val(row("QvP")) Then
    '        '    Curve.Values.Add(New EsPLValue2(Val(row("QvP")), TopLevel - row("Level")))
    '        '    'End If
    '        'Next
    '        PressCurveTable.GridMinX = 0
    '        PressCurveTable.GridMaxX = If(LimitValue * ResultRangeMultiple Mod 100 = 0, LimitValue * ResultRangeMultiple, 100 * (Fix(LimitValue * ResultRangeMultiple / 100) + 1)) 'LimitValue / SpudcanArea * 2 
    '        Dim Rows As DataRow() = MyDataSet.Tables("LS_PressResistanceResult").Select("DrillingID=" & DrillingIDs(IDi) & " And QvP<>'-'" & If(BoatID = -1, "", " and BoatID=" & BoatID), "Level DESC")
    '        For i = 0 To Rows.Count - 1
    '            If PressCurveTable.GridMaxX >= Val(Rows(i)("QvP")) Then
    '                Curve.Values.Add(New EsPLValue2(Val(Rows(i)("QvP")), TopLevel - Rows(i)("Level")))
    '            Else
    '                If i <> 0 AndAlso PressCurveTable.GridMaxX >= Val(Rows(i - 1)("QvP")) Then
    '                    Dim MLevel As Double = Val(Rows(i)("Level")) - (Rows(i)("QvP") - PressCurveTable.GridMaxX) / (Val(Rows(i)("QvP")) - Val(Rows(i - 1)("QvP"))) * (Rows(i)("Level") - Rows(i - 1)("Level"))
    '                    Curve.Values.Add(New EsPLValue2(PressCurveTable.GridMaxX, TopLevel - MLevel))
    '                End If
    '                If i <> Rows.Count - 1 AndAlso PressCurveTable.GridMaxX >= Val(Rows(i + 1)("QvP")) Then
    '                    Dim MLevel As Double = Val(Rows(i + 1)("Level")) - (Rows(i + 1)("QvP") - PressCurveTable.GridMaxX) / (Val(Rows(i + 1)("QvP")) - Val(Rows(i)("QvP"))) * (Rows(i + 1)("Level") - Rows(i)("Level"))
    '                    Curve.Values.Add(New EsPLValue2(PressCurveTable.GridMaxX, TopLevel - MLevel))
    '                End If
    '            End If
    '        Next
    '        Dim MustShowV2s As List(Of Double) = GetLayerLevels(MyDataSet, DrillingIDs(IDi), BoatID)
    '        For Each MSY In MustShowV2s
    '            For Each AValue In Curve.Values
    '                If AValue.V2 = TopLevel - MSY Then
    '                    If Not Curve.MustShowPoints.Contains(AValue) Then Curve.MustShowPoints.Add(AValue)
    '                    Exit For
    '                End If
    '            Next
    '        Next
    '        Dim QvP As Double
    '        Dim Qv As Double = -10 ^ 5
    '        Dim QvPTypeDes As String() = If(DrillingIDs.Count = 1, {"推荐参数承载力"}, {"最小承载力（小值)", "推荐参数承载力（中值）", "最大承载力（大值）"})
    '        Dim SpudcanArea As Double
    '        If MyDataSet.Tables("LS_Common").Columns.Contains("UseSingleDrilling") Then
    '            SpudcanArea = Val(MyDataSet.Tables("LS_Spudcan").Rows(0)("Area"))
    '            If MyDataSet.Tables("LS_DepthResult").Select("DrillingID=" & DrillingIDs(IDi), "").Count > 0 Then
    '                Qv = Val(MyDataSet.Tables("LS_DepthResult").Select("DrillingID=" & DrillingIDs(IDi), "")(0)("Qv"))
    '            End If
    '        Else
    '            SpudcanArea = Double.Parse(MyDataSet.Tables("LS_Boat").Select("ID=" & BoatID)(0)("SpudcanA").ToString)
    '            If MyDataSet.Tables("LS_Boat").Select("ID=" & BoatID).Count > 0 Then
    '                Qv = Double.Parse(MyDataSet.Tables("LS_DepthResult").Select("DrillingID=" & DrillingIDs(IDi) & If(BoatID = -1, "", " and BoatID=" & BoatID), "")(0)("Qv").ToString)
    '            End If
    '        End If
    '        QvP = Round(Qv / SpudcanArea, 2)
    '        Curve.Legend = QvPTypeDes(IDi) & If(Qv = -10 ^ 5, "无匹配值", QvP & " kPa")  '结果类型（中值-推荐值、大值、小值）20251010
    '        'Curve.Legendlocation
    '        PressCurveTable.Curves.Add(Curve)

    '    Next

    '    PressCurveTable.GridDY = 5
    '    PressCurveTable.GridMinY = 0
    '    PressCurveTable.GridMaxY = If(PressCurveTable.Curves(0).Values.Last.V2 Mod 10 = 0, PressCurveTable.Curves(0).Values.Last.V2, 10 * (Fix(PressCurveTable.Curves(0).Values.Last.V2 / 10) + 1))

    '    Dim LVlc As EsPLColor() = {New EsPLColor(1, 0, 0), New EsPLColor(242 / 255, 186 / 255, 2 / 255), New EsPLColor(72 / 255, 116 / 255, 203 / 255)} '红黄蓝
    '    For i = 0 To LimitValueShowMultiple.Count - 1 '20250929增加两根倍数对地比压的竖线曲线，倍数值可修改
    '        Dim LV As Double = LimitValueShowMultiple(i) * LimitValue
    '        Curve = New EsPLCurve
    '        Curve.Color = LVlc(i)
    '        Curve.LineWidth = 2
    '        Curve.LineStyle = EsPLLineStyle.Dash
    '        Curve.MarkType = ESPLMarkType.None
    '        Curve.ShowPointText = False
    '        Curve.Legend = If(LimitValueShowMultiple(i) <> 1, LimitValueShowMultiple(i) & "倍对地比压", "对地比压" & Round(LimitValue, 2) & "(kPa)")  '"(kN)"
    '        Curve.CurveNameFontSize = 12
    '        Curve.Values.Add(New EsPLValue2(Round(LV, 2), PressCurveTable.Curves(0).Values.First.V2)) '- XSep'与副Y坐标相关
    '        Curve.Values.Add(New EsPLValue2(Round(LV, 2), PressCurveTable.Curves(0).Values.Last.V2)) '+ XSep
    '        PressCurveTable.Curves.Add(Curve)
    '    Next

    '    PressCurveTable.VerticalValue0 = TopLevel
    '    PressCurveTable.YValueDirection = -1
    '    PressCurveTable.VerticalValueCoeff = -1
    '    For Each row As DataRow In MyDataSet.Tables("LS_PressResistanceResult").Select("DrillingID=" & DrillingID & " and Qv<>'-'" & If(BoatID = -1, "", " and BoatID=" & BoatID), "Level DESC")
    '        PressCurveTable.HorizontalValue0 = PressCurveTable.GridMinX ' Val(row("QV"))
    '        PressCurveTable.HorizontalValueCoeff = Val(row("QV")) / Val(row("QVP"))
    '        Exit For
    '    Next
    '    PressCurveTable.NXSeparate = XSep
    '    PressCurveTable.NYSeparate = YSep
    '    Return PressCurveTable.DrawCurve(Width, Height, UseMetaFile)
    'End Function

    'Shared Function DrawPullCurve(MyDataSet As DataSet, PullCurveTable As EsPLCurveTable, Width As Double, Height As Double, Optional XSep As Double = 1, Optional YSep As Double = 1, Optional DrillingID As Integer = 1, Optional BoatID As Integer = -1, Optional UseMetaFile As Boolean = True) As Drawing.Image 'LimitValue As Double,
    '    On Error Resume Next
    '    Dim DrillingName As String = GetDrillingName(MyDataSet, DrillingID, BoatID)
    '    '
    '    PullCurveTable.ScaleFontSize = 10
    '    PullCurveTable.ScaleFontColor = New EsPLColor(0, 176 / 255, 240 / 255) 'WPS文字浅蓝
    '    PullCurveTable.ScaleHorizontalAlternateShow = True
    '    PullCurveTable.ScaleVerticalAlternateShow = False
    '    PullCurveTable.VerticalTitle.FontSize = 12
    '    PullCurveTable.VerticalTitle.Text = "插深(m)"
    '    PullCurveTable.VerticalTitle2 = "桩靴底标高(m)"
    '    PullCurveTable.VerticalTitleFormat2 = "0.00"
    '    PullCurveTable.VerticalTitle.Alignment = EsPLTextAlignment.Center
    '    PullCurveTable.VerticalTitle.LineAlignment = EsPLTextAlignment.Right
    '    PullCurveTable.VerticalTitle.Angle = -90

    '    PullCurveTable.HorizontalTitle.FontSize = 12
    '    PullCurveTable.HorizontalTitle.Text = DrillingName & "抗拔力Qu(t)" '"极限抗拔承载力(kN)"
    '    PullCurveTable.HorizontalTitle2 = DrillingName & "抗拔力Qu(kN)"
    '    PullCurveTable.HorizontalTitle.Alignment = EsPLTextAlignment.Center
    '    PullCurveTable.HorizontalTitle.LineAlignment = EsPLTextAlignment.Left
    '    PullCurveTable.HorizontalTitleFormat2 = "0"

    '    PullCurveTable.WithGrid = True
    '    PullCurveTable.GridLineStyle = EsPLLineStyle.Solid
    '    PullCurveTable.GridLineColor = New EsPLColor(0.918, 0.918, 0.918) '灰色

    '    PullCurveTable.Curves.Clear()

    '    Dim Curve As New EsPLCurve
    '    Curve.Color = New EsPLColor(0, 0, 0)
    '    Curve.LineWidth = 2
    '    Curve.MarkType = ESPLMarkType.Point
    '    Curve.MarkColor = New EsPLColor(91 / 255, 155 / 255, 213 / 255) '0, 0, 1
    '    Curve.ShowPointText = True
    '    Curve.ShowCurveValueStyle = EsPLShowCurveValueStyle.XValue
    '    Dim TopLevel As Double = MyDataSet.Tables("LS_CalculationLevels").Compute("Max(Level)", "DrillingID=" & DrillingID & If(BoatID = -1, "", " and BoatID=" & BoatID))
    '    'Dim TopLevel As Double = MyDataSet.Tables("LS_LegSoilLayer").Compute("Max(TopLevel)", "")
    '    For Each row As DataRow In MyDataSet.Tables("LS_PullResistanceResult").Select("DrillingID=" & DrillingID & " And QuP<>'-'" & If(BoatID = -1, "", " and BoatID=" & BoatID), "Level DESC")
    '        Curve.Values.Add(New EsPLValue2(row("QuP"), TopLevel - row("Level")))
    '    Next
    '    Dim MustShowV2s As List(Of Double) = GetLayerLevels(MyDataSet, DrillingID, BoatID)
    '    For Each MSY In MustShowV2s
    '        For Each AValue In Curve.Values
    '            If AValue.V2 = TopLevel - MSY Then
    '                If Not Curve.MustShowPoints.Contains(AValue) Then Curve.MustShowPoints.Add(AValue)
    '                Exit For
    '            End If
    '        Next
    '    Next
    '    PullCurveTable.Curves.Add(Curve)

    '    'Curve = New EsPLCurve
    '    'Curve.Color = New EsPLColor(1, 0, 0)
    '    'Curve.LineWidth = 2
    '    'Curve.LineStyle = EsPLLineStyle.Dash
    '    'Curve.MarkType = ESPLMarkType.None
    '    'Curve.ShowPointText = False
    '    'Curve.CurveName = LimitValue & "(kN)"
    '    'Curve.Values.Add(New EsPLValue2(LimitValue, PullCurveTable.Curves(0).Values.First.V2 - XSep))
    '    'Curve.Values.Add(New EsPLValue2(LimitValue, PullCurveTable.Curves(0).Values.Last.V2 + XSep))
    '    'PullCurveTable.Curves.Add(Curve)

    '    PullCurveTable.VerticalValue0 = TopLevel
    '    PullCurveTable.YValueDirection = -1
    '    PullCurveTable.VerticalValueCoeff = -1
    '    For Each row As DataRow In MyDataSet.Tables("LS_PullResistanceResult").Select("DrillingID=" & DrillingID & " And Qu<>'-'" & If(BoatID = -1, "", " and BoatID=" & BoatID), "Level DESC")
    '        PullCurveTable.HorizontalValue0 = row("Qu")
    '        PullCurveTable.HorizontalValueCoeff = row("Qu") / row("QuP")
    '        Exit For
    '    Next
    '    PullCurveTable.NXSeparate = XSep
    '    PullCurveTable.NYSeparate = YSep
    '    Return PullCurveTable.DrawCurve(Width, Height, UseMetaFile)
    'End Function

    Sub DrawLeg(ByVal ShapeList As EsShapeList)
        Dim AShape As EsShape
        AShape = New EsShape
        AShape.ID = 1







        ShapeList.Shapes.Add(AShape)
    End Sub
    Sub DrawSoilLayers(ByVal ShapeList As EsShapeList)
        Dim LayerMeshs As List(Of EsTLLayerMesh) = GetSoilLayerMesh()
        If LayerMeshs.Count > 0 Then
            Dim BPShapes As List(Of EsTLBPShape) = EsTLSoilGeometryKit.GenSoilBPObjectByTriPrismAndMerge(LayerMeshs)
            Dim P As EsTLPoint, i As Integer
            Dim Colors(,) As Double = {{0.5, 0, 0.5}, {0.0, 0.5, 0.5}, {0.5, 0, 0}, {0.0, 0.5, 0}, {0, 0, 0.5}, {0.5, 0.5, 0}}
            For Each BPShape As EsTLBPShape In BPShapes
                Dim AShape As New EsShape
                AShape.ID = 100 + i
                Dim IColor As Integer = i Mod 6
                Dim PolygonFace As New EsPolygonFaceShape
                PolygonFace.Style.Color.Red = Colors(IColor, 0)
                PolygonFace.Style.Color.Green = Colors(IColor, 1)
                PolygonFace.Style.Color.Blue = Colors(IColor, 2)
                For Each Face As EsTLBPFace In BPShape.Faces
                    Dim Polygon As New EsPolygon
                    Polygon.Normal.Vx = Face.Surface.Axis.Vz.Vx * If(Face.SurfaceOrientation = 0, 1, -1)
                    Polygon.Normal.Vy = Face.Surface.Axis.Vz.Vy * If(Face.SurfaceOrientation = 0, 1, -1)
                    Polygon.Normal.Vz = Face.Surface.Axis.Vz.Vz * If(Face.SurfaceOrientation = 0, 1, -1)
                    For Each EdgeLoop As EsTLBPEdge In Face.Loops(0).Edges
                        If EdgeLoop.Orientation = 0 Then
                            P = EdgeLoop.Edge.P1
                        Else
                            P = EdgeLoop.Edge.P2
                        End If
                        Polygon.Vertexs.Add(New EsPoint(P.x, P.y, P.z))
                    Next
                    PolygonFace.Shapes.Add(Polygon)
                Next
                AShape.PolygonFaces.Add(PolygonFace)
                ShapeList.Shapes.Add(AShape)
                i += 1
            Next
        End If
    End Sub
End Class

