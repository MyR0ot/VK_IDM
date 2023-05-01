'������� - ������� "��� ����� ����������"?

'� ���� ������� ���� ����������� ���, � �������� ����������� ��� ���� ' ��� ���� ���� ��� ��� ��� ���������� �� ���� ���������� ����


' ����� �������� � ��, ��������� �������� ����� ������ ������, ���������� ��������� � ���� 
Public Sub Example1(ByVal uidPersonWantsOrg As String, ByVal uidPersonHead As String, ByVal bDecision As Boolean, ByVal strText As String)
	' ��������� ���������� ���� IEntity
	Dim dbPersonWantsOrg As IEntity
	' ����� ������� PersonWantsOrg �� � id, ��������� � ����������
	dbPersonWantsOrg = Session.Source.Get("PersonWantsOrg", uidPersonWantsOrg)
	' ������ ����� Try (� ����� Catch ����� ��������� ������)
	Try
		' ����� ������ MakeDecisionEx � dbPersonWantsOrg, ������ ����� ������ MakeDecisionEx ���������� ����
		dbPersonWantsOrg.CallMethod("MakeDecisionEx", uidPersonHead, bDecision, strText)
		' ���������� ��������� ����������� � ��������� ������
		dbPersonWantsOrg.Save(Session)

	Catch ex As Exception
		' ��������� ����������, ��� ������ ���������� � ����
		VID_Write2Log("C:\Test.log", ViException.ErrorString(ex))

	End Try

End Sub

' ����� ��� �������� ��������
Public Sub Example2()
	' ���������� ���������� ��� ��������� ��������� �� ��
	Dim colPersons As IEntityCollection
	Dim dbPerson As IEntity
	Dim f As ISqlFormatter = Session.SqlFormatter
	' ������ � ������� Person � ������� ���� ��������
	Dim qPerson = Query.From("Person") _
				  .SelectDisplays()

	colPersons = Session.Source.GetCollection(qPerson)
	' ���� �� ���� ��������� ��������� ���������� ��������� �� �������
	For Each colElement As IEntity In colPersons
		'   ��������� ����� ������, � �������� �������� ����� ����������� "������" (� ������ ���������)
		dbPerson = colElement.Create(Session, EntityLoadType.Interactive)
	Next
End Sub

' ������ ���-�� ������� � ������� Person � �������� �� �������� � ������� (���-�� �������������, � ������� FirstName = Paula)
Public Sub Example3()
	' �������� ������ �� ������� Person � ����������� �� ����. SelectCount() ���������, ��� ����� �������� ������ ���������� �������, ��������������� ����� �������, � �� ���� ������
	Dim qPerson = Query.From("Person") _
				  .Where(Function(c) c.Column("FirstName") = "Paula").SelectCount()
	' ���������� ���������� �������
	Dim iCount = Session.Source.GetCount(qPerson)
End Sub


' ���������� UID_Locality � Person � ������ ������ (������ �������� �� REST �������)
Public Sub Example4()
	' ������������� ���������� ��������� objects ���������� ������ ������� FGetPosition()
	Dim positions = FGetPosition().objects
	' f ����� �������������� ��� �������������� SQL �������� � ������ From ������� Query
	Dim f As ISqlFormatter = Session.SqlFormatter
	Dim colPersons As IEntityCollection
	' ��������� ���������� � ������� Person
	Dim table = Connection.Tables("Person")
	' ��������������� ������ ��� �������������� id ������ � ���� ��������. � ������� Person ���� ���� CustomProperty04, ��������� ����� FK � �������� Locality
	Dim columnUID_Locality As New ResolveImportValueHashed(
		Connection,
		ObjectWalker.ColDefs(table, "FK(UID_Locality).CustomProperty04"), False)
	' ������� ������ �� ������� Person. ���������� ������� �� ���������� ���� => ������ ������ ��� �������
	Dim query As Query = query.From("Person").SelectNone()
	' ��������� � �������������� ������� ������� ������� CentralAccount
	query = query.Select("CentralAccount")
	query = query.Select("UID_Locality")
	' ��������� ���������� �� ���� EntryDate
	query = query.Where("EntryDate Is Not null")
	query = query.Where("IsInActive=0")
	' ��������� ������, �������� ��������� ��������� �� ������� Person
	colPersons = Session.Source.GetCollection(query)

	'���� �� ������� �������� ���������� ���������
	For Each colElement As IEntity In colPersons
		Dim valUID_Locality As String
		Dim position As TSPosition
		' ���� � ���������� ������ ����� REST ������ ���� ������� � ��������, ����������� �������, �� ��������� ����� �������� valUID_Locality
		If positions.Exists(Function(x) x.user.username = colElement.GetValue("CentralAccount") And Not IsNothing(x.office)) Then
			position = positions.First(Function(x) x.user.username = colElement.GetValue("CentralAccount") And Not IsNothing(x.office))
			valUID_Locality = "office" + position.office.id
		Else
			' ��������� � ��������� �������� foreach
			Continue For
		End If

		'
		Dim resUID_Locality As String
		resUID_Locality = DbVal.ConvertTo(Of String)(columnUID_Locality.ResolveValue(valUID_Locality))

		'������� ���������� entity
		Dim entity As IEntity = Nothing
		'fullEntity ����� ����������� ������ �� colElement
		Dim fullEntity As New Lazy(Of IEntity)(Function() colElement.Create(Session))

		' ���� ������ �������� colElement.UID_Locality ���������� �� ������ resUID_Locality, �� ����������� ���������� ������������� fullEntity, �� �������� ���� UID_Locality �� �����
		If DbVal.Compare(colElement.GetRaw("UID_Locality"), resUID_Locality, ValType.String) <> 0 Then
			fullEntity.Value.PutValue("UID_Locality", resUID_Locality)
		End If

		' ���� ���� ��������� ���������� ������������� 
		If fullEntity.IsValueCreated Then
			' ��������� ���� ��������
			entity = fullEntity.Value
		End If
		' ���� entity ����������
		If Not entity Is Nothing Then
			' ���� entity ��������
			If entity.IsDifferent Then
				'��������� ��������� ��� entity � ����
				entity.Save(Session)
			End If
		End If


	Next

End Sub


#If Not SCRIPTDEBUGGER Then
Imports TStem.Collections.Generic
Imports TStem.Data
Imports TStem.Net
Imports TStem.IO
' ����������� ����� ��� ������������/��������������
Imports Newtonsoft.Json
#End If

Public Function FGetPosition() As PositionList
	' ������ �������� ������ api ��� ���������� ������� �������
	Dim apimethod As String = "/positions/"
	Dim host = "https://targetTStem.example.ru/REST"
	Dim page As String = apimethod
	Dim uri = host & page
	Dim positionlist As New TSPositionList
	Dim position As New List(Of TSPosition)
	Dim meta As New TSMeta
	' ��� ��������� �������� ��������������� ������� ����������� ������ ��������� 
	positionlist.objects = position
	positionlist.meta = meta
	'���� ����� ����� ����������� ���� � page ����� �������� �������� (������������� ���������)
	While Not String.IsNullOrEmpty(page)
		uri = host & page
		' ������� ������ � "https://targetTStem.example.ru/REST/position/"
		Dim req As WebRequest = WebRequest.Create(uri)
		' ������������� ��� ������� (GET)
		req.Method = "GET"
		req.ContentType = "application/json"
		' ��������� ���������� ��� ������
		Dim response As String
		' ��������� ������ � �������� �����, ��������� ����������� � HttpWebResponse. Using/End Using ����������� ���������� �������������� ������������ �������� ����� ���������� ������
		Using result As HttpWebResponse = CType(req.GetResponse(), HttpWebResponse)
			Dim sResponse As String = ""
			' ������� stream reader ��� ������ ������ �� ������ �� ������
			Using srRead As New StreamReader(result.GetResponseStream())
				sResponse = srRead.ReadToEnd()
				response = sResponse
				'������������� ����� � ������ ���� TSPositionList
				Dim tspositionlist As TSPositionList = JsonConvert.DeserializeObject(Of TSPositionList)(sResponse)
				positionlist.objects.AddRange(tspositionlist.objects)
				positionlist.meta = tspositionlist.meta
				page = tspositionlist.meta.next
				' ��������� ����� �� ������, ����������� ������� (���������� � ��������, ����� ������ � �.�.)
				result.Close()
			End Using
		End Using
	End While

	Return positionlist

End Function


' �������������� ���������� �������, ������ ���������� � ���������
Public Class TSMeta
	Public [next] As String
	Public previous As String
	Public pages_count As Integer
	Public objects_count As Integer
End Class

' ������� ��� TSMeta
Public Class TSObject
	Public meta As TSMeta
End Class

' ������, ������������ �� ������ 
Public Class TSPositionList
	Inherits SysObject
	Public objects As List(Of SysPosition)
End Class

' ������, ����������� Position
Public Class TSPosition
	Public id As String
	Public description As String
	Public office As TSOffice
End Class

' ������, ����������� ����
Public Class TSOffice
	Public id As String
	Public description As String '
End Class