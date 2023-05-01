'Задание - объясни "что здесь происходит"?

'в этом задании надо рассмотреть код, и добавить комментарии где есть ' для чего этот код или что происходит по ходу выполнения кода


' Поиск сущности в БД, изменение сущности путем вызова метода, сохранение изменений в базу 
Public Sub Example1(ByVal uidPersonWantsOrg As String, ByVal uidPersonHead As String, ByVal bDecision As Boolean, ByVal strText As String)
	' объявляем переменную типа IEntity
	Dim dbPersonWantsOrg As IEntity
	' Поиск сущости PersonWantsOrg по её id, сохраняем в переменную
	dbPersonWantsOrg = Session.Source.Get("PersonWantsOrg", uidPersonWantsOrg)
	' Начало блока Try (в блоке Catch будет обработка ошибка)
	Try
		' вызов метода MakeDecisionEx у dbPersonWantsOrg, скорее всего внутри MakeDecisionEx изменяются поля
		dbPersonWantsOrg.CallMethod("MakeDecisionEx", uidPersonHead, bDecision, strText)
		' Результаты изменений сохраняются в источнике данных
		dbPersonWantsOrg.Save(Session)

	Catch ex As Exception
		' обработка исключений, все ошибки логируются в файл
		VID_Write2Log("C:\Test.log", ViException.ErrorString(ex))

	End Try

End Sub

' Метод без полезной нагрузки
Public Sub Example2()
	' Объявление переменной для коллекции сущностей из БД
	Dim colPersons As IEntityCollection
	Dim dbPerson As IEntity
	Dim f As ISqlFormatter = Session.SqlFormatter
	' запрос к таблице Person с выбором всех столбцов
	Dim qPerson = Query.From("Person") _
				  .SelectDisplays()

	colPersons = Session.Source.GetCollection(qPerson)
	' Цикл по всем элементам коллекции полученной коллекции из запроса
	For Each colElement As IEntity In colPersons
		'   сохраняем новый объект, у которого свойства будут вычисляться "лениво" (в момент обращения)
		dbPerson = colElement.Create(Session, EntityLoadType.Interactive)
	Next
End Sub

' Расчет кол-ва записей в таблице Person с условием по значению в колонке (кол-во пользователей, у которых FirstName = Paula)
Public Sub Example3()
	' Выбираем данные из таблицы Person с фильтрацией по полю. SelectCount() указывает, что нужно получить только количество записей, удовлетворяющих этому запросу, а не сами записи
	Dim qPerson = Query.From("Person") _
				  .Where(Function(c) c.Column("FirstName") = "Paula").SelectCount()
	' полученние результата запроса
	Dim iCount = Session.Source.GetCount(qPerson)
End Sub


' Обновление UID_Locality в Person в каждой записи (данные получаем из REST запроса)
Public Sub Example4()
	' Инициализация переменной свойством objects результата работы функции FGetPosition()
	Dim positions = FGetPosition().objects
	' f будет использоваться для форматирования SQL запросов в методе From объекта Query
	Dim f As ISqlFormatter = Session.SqlFormatter
	Dim colPersons As IEntityCollection
	' получение информации о табилце Person
	Dim table = Connection.Tables("Person")
	' вспомогательный объект для преобразования id строки в само значение. У таблицы Person есть поле CustomProperty04, связанное через FK с таблицей Locality
	Dim columnUID_Locality As New ResolveImportValueHashed(
		Connection,
		ObjectWalker.ColDefs(table, "FK(UID_Locality).CustomProperty04"), False)
	' создаем запрос из таблицы Person. Выбираемые столбцы не определены явно => запрос вернет все столбцы
	Dim query As Query = query.From("Person").SelectNone()
	' Добавляем в результирующую выборку запроса колонку CentralAccount
	query = query.Select("CentralAccount")
	query = query.Select("UID_Locality")
	' добавляем фильтрацию по полю EntryDate
	query = query.Where("EntryDate Is Not null")
	query = query.Where("IsInActive=0")
	' выполняем запрос, получаем коллекцию элементов из таблицы Person
	colPersons = Session.Source.GetCollection(query)

	'цикл по каждому элементу полученной коллекции
	For Each colElement As IEntity In colPersons
		Dim valUID_Locality As String
		Dim position As TSPosition
		' если в полученных данных через REST запрос есть элемент с условием, описываемым лямбдой, то вычисляем новое значение valUID_Locality
		If positions.Exists(Function(x) x.user.username = colElement.GetValue("CentralAccount") And Not IsNothing(x.office)) Then
			position = positions.First(Function(x) x.user.username = colElement.GetValue("CentralAccount") And Not IsNothing(x.office))
			valUID_Locality = "office" + position.office.id
		Else
			' переходим к следующей итерации foreach
			Continue For
		End If

		'
		Dim resUID_Locality As String
		resUID_Locality = DbVal.ConvertTo(Of String)(columnUID_Locality.ResolveValue(valUID_Locality))

		'заводим переменную entity
		Dim entity As IEntity = Nothing
		'fullEntity будет заполняться лениво из colElement
		Dim fullEntity As New Lazy(Of IEntity)(Function() colElement.Create(Session))

		' если старое значение colElement.UID_Locality отличается от нового resUID_Locality, то выполняется отложенная инициализация fullEntity, мы изменяем поле UID_Locality на новое
		If DbVal.Compare(colElement.GetRaw("UID_Locality"), resUID_Locality, ValType.String) <> 0 Then
			fullEntity.Value.PutValue("UID_Locality", resUID_Locality)
		End If

		' если была выполнены отложенная инициализация 
		If fullEntity.IsValueCreated Then
			' сохраняем само значение
			entity = fullEntity.Value
		End If
		' Если entity определено
		If Not entity Is Nothing Then
			' Если entity изменено
			If entity.IsDifferent Then
				'сохраняем изменения для entity в базе
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
' импортируем пакет для сериализации/десериализации
Imports Newtonsoft.Json
#End If

Public Function FGetPosition() As PositionList
	' задаем название метода api для выполнения первого запроса
	Dim apimethod As String = "/positions/"
	Dim host = "https://targetTStem.example.ru/REST"
	Dim page As String = apimethod
	Dim uri = host & page
	Dim positionlist As New TSPositionList
	Dim position As New List(Of TSPosition)
	Dim meta As New TSMeta
	' для коллекции объектов результирующего объекта присваиваем пустую коллекцию 
	positionlist.objects = position
	positionlist.meta = meta
	'тело цикла будет выполняться пока в page будет непустое значение (предусмотрена пагинация)
	While Not String.IsNullOrEmpty(page)
		uri = host & page
		' создаем запрос к "https://targetTStem.example.ru/REST/position/"
		Dim req As WebRequest = WebRequest.Create(uri)
		' Устанавливаем тип запроса (GET)
		req.Method = "GET"
		req.ContentType = "application/json"
		' Объявляем переменную для ответа
		Dim response As String
		' выполняем запрос и получаем ответ, результат преобразуем в HttpWebResponse. Using/End Using гарантирует корректное автоматическое освобождение ресурсов после завершения секции
		Using result As HttpWebResponse = CType(req.GetResponse(), HttpWebResponse)
			Dim sResponse As String = ""
			' Создаем stream reader для чтения данных из ответа на запрос
			Using srRead As New StreamReader(result.GetResponseStream())
				sResponse = srRead.ReadToEnd()
				response = sResponse
				'десериализуем ответ в модель типа TSPositionList
				Dim tspositionlist As TSPositionList = JsonConvert.DeserializeObject(Of TSPositionList)(sResponse)
				positionlist.objects.AddRange(tspositionlist.objects)
				positionlist.meta = tspositionlist.meta
				page = tspositionlist.meta.next
				' Закрываем ответ на запрос, освобождаем ресурсы (соединение с сервером, поток данных и т.п.)
				result.Close()
			End Using
		End Using
	End While

	Return positionlist

End Function


' метаинформация результата запроса, хранит информацию о пагинации
Public Class TSMeta
	Public [next] As String
	Public previous As String
	Public pages_count As Integer
	Public objects_count As Integer
End Class

' Обертка над TSMeta
Public Class TSObject
	Public meta As TSMeta
End Class

' Модель, возвращаемая на запрос 
Public Class TSPositionList
	Inherits SysObject
	Public objects As List(Of SysPosition)
End Class

' Модель, описывающая Position
Public Class TSPosition
	Public id As String
	Public description As String
	Public office As TSOffice
End Class

' Модель, описывающая офис
Public Class TSOffice
	Public id As String
	Public description As String '
End Class