<?xml version="1.0" encoding="utf-8"?>
<configurationSectionModel xmlns:dm0="http://schemas.microsoft.com/VisualStudio/2008/DslTools/Core" dslVersion="1.0.0.0" Id="d0ed9acb-0435-4532-afdd-b5115bc4d562" namespace="ApiServiceEngine.Configuration" xmlSchemaNamespace="ApiServiceEngine" xmlns="http://schemas.microsoft.com/dsltools/ConfigurationSectionDesigner">
  <typeDefinitions>
    <externalType name="String" namespace="System" />
    <externalType name="Boolean" namespace="System" />
    <externalType name="Int32" namespace="System" />
    <externalType name="Int64" namespace="System" />
    <externalType name="Single" namespace="System" />
    <externalType name="Double" namespace="System" />
    <externalType name="DateTime" namespace="System" />
    <externalType name="TimeSpan" namespace="System" />
    <externalType name="FbDbType" namespace="FirebirdSql.Data.FirebirdClient" />
    <enumeratedType name="RequestMethod" namespace="ApiServiceEngine.Configuration">
      <literals>
        <enumerationLiteral name="Get" />
        <enumerationLiteral name="Post" />
      </literals>
    </enumeratedType>
  </typeDefinitions>
  <configurationElements>
    <configurationSection name="ApiSection" codeGenOptions="Singleton, XmlnsProperty" xmlSectionName="api">
      <elementProperties>
        <elementProperty name="Tasks" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="tasks" isReadOnly="false" documentation="Список доступных задач">
          <type>
            <configurationElementCollectionMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/Tasks" />
          </type>
        </elementProperty>
        <elementProperty name="Services" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="services" isReadOnly="false" documentation="Список доступных методов">
          <type>
            <configurationElementCollectionMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/Services" />
          </type>
        </elementProperty>
      </elementProperties>
    </configurationSection>
    <configurationElement name="RunMethod">
      <attributeProperties>
        <attributeProperty name="Method" isRequired="true" isKey="true" isDefaultCollection="false" xmlName="method" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/String" />
          </type>
        </attributeProperty>
      </attributeProperties>
    </configurationElement>
    <configurationElementCollection name="RunMethods" collectionType="AddRemoveClearMap" xmlItemName="runMethod" codeGenOptions="Indexer, AddMethod, RemoveMethod, GetItemMethods">
      <itemType>
        <configurationElementMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/RunMethod" />
      </itemType>
    </configurationElementCollection>
    <configurationElement name="Task">
      <attributeProperties>
        <attributeProperty name="Name" isRequired="true" isKey="true" isDefaultCollection="false" xmlName="name" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/String" />
          </type>
        </attributeProperty>
      </attributeProperties>
      <elementProperties>
        <elementProperty name="Methods" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="methods" isReadOnly="false">
          <type>
            <configurationElementCollectionMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/RunMethods" />
          </type>
        </elementProperty>
      </elementProperties>
    </configurationElement>
    <configurationElementCollection name="Tasks" xmlItemName="task" codeGenOptions="Indexer, AddMethod, RemoveMethod, GetItemMethods">
      <itemType>
        <configurationElementMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/Task" />
      </itemType>
    </configurationElementCollection>
    <configurationElement name="Parameter">
      <attributeProperties>
        <attributeProperty name="Name" isRequired="true" isKey="true" isDefaultCollection="false" xmlName="name" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="ApiName" isRequired="true" isKey="false" isDefaultCollection="false" xmlName="api_name" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="IsPage" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="is_page" isReadOnly="false" defaultValue="false">
          <type>
            <externalTypeMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/Boolean" />
          </type>
        </attributeProperty>
        <attributeProperty name="Index" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="index" isReadOnly="false" defaultValue="0">
          <type>
            <externalTypeMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/Int32" />
          </type>
        </attributeProperty>
        <attributeProperty name="Required" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="required" isReadOnly="false" defaultValue="true">
          <type>
            <externalTypeMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/Boolean" />
          </type>
        </attributeProperty>
        <attributeProperty name="IsList" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="is_list" isReadOnly="false" defaultValue="false">
          <type>
            <externalTypeMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/Boolean" />
          </type>
        </attributeProperty>
      </attributeProperties>
      <elementProperties>
        <elementProperty name="Db" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="db" isReadOnly="false">
          <type>
            <configurationElementMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/DbProperty" />
          </type>
        </elementProperty>
        <elementProperty name="Recive" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="recive" isReadOnly="false">
          <type>
            <configurationElementMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/Recive" />
          </type>
        </elementProperty>
      </elementProperties>
    </configurationElement>
    <configurationElement name="DbProperty">
      <attributeProperties>
        <attributeProperty name="Field" isRequired="true" isKey="true" isDefaultCollection="false" xmlName="field" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="Type" isRequired="true" isKey="false" isDefaultCollection="false" xmlName="type" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/FbDbType" />
          </type>
        </attributeProperty>
        <attributeProperty name="Length" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="length" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/Int32" />
          </type>
        </attributeProperty>
      </attributeProperties>
    </configurationElement>
    <configurationElementCollection name="Out" xmlItemName="parameter" codeGenOptions="Indexer, AddMethod, RemoveMethod, GetItemMethods">
      <itemType>
        <configurationElementMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/Parameter" />
      </itemType>
    </configurationElementCollection>
    <configurationElementCollection name="In" xmlItemName="parameter" codeGenOptions="Indexer, AddMethod, RemoveMethod, GetItemMethods">
      <itemType>
        <configurationElementMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/Parameter" />
      </itemType>
    </configurationElementCollection>
    <configurationElement name="Method">
      <attributeProperties>
        <attributeProperty name="Name" isRequired="true" isKey="true" isDefaultCollection="false" xmlName="name" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="ApiName" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="apiName" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="Procedure" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="procedure" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="Version" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="version" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/Int32" />
          </type>
        </attributeProperty>
        <attributeProperty name="Request" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="request" isReadOnly="false" defaultValue="RequestMethod.Get">
          <type>
            <enumeratedTypeMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/RequestMethod" />
          </type>
        </attributeProperty>
      </attributeProperties>
      <elementProperties>
        <elementProperty name="In" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="in" isReadOnly="false">
          <type>
            <configurationElementCollectionMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/In" />
          </type>
        </elementProperty>
        <elementProperty name="Out" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="out" isReadOnly="false">
          <type>
            <configurationElementCollectionMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/Out" />
          </type>
        </elementProperty>
      </elementProperties>
    </configurationElement>
    <configurationElementCollection name="Methods" xmlItemName="method" codeGenOptions="Indexer, AddMethod, RemoveMethod, GetItemMethods">
      <itemType>
        <configurationElementMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/Method" />
      </itemType>
    </configurationElementCollection>
    <configurationElement name="Settings">
      <attributeProperties>
        <attributeProperty name="Url" isRequired="true" isKey="false" isDefaultCollection="false" xmlName="url" isReadOnly="false" documentation="Адрес">
          <type>
            <externalTypeMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="Login" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="login" isReadOnly="false" documentation="Имя пользователя">
          <type>
            <externalTypeMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="Password" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="password" isReadOnly="false" documentation="Пароль">
          <type>
            <externalTypeMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/String" />
          </type>
        </attributeProperty>
      </attributeProperties>
    </configurationElement>
    <configurationElement name="Service" documentation="Описание сервиса (способ доступа, методы)">
      <attributeProperties>
        <attributeProperty name="Name" isRequired="true" isKey="true" isDefaultCollection="false" xmlName="name" isReadOnly="false" documentation="Наименование сервиса">
          <type>
            <externalTypeMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="Comment" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="comment" isReadOnly="false" documentation="Описание сервиса">
          <type>
            <externalTypeMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="Type" isRequired="true" isKey="false" isDefaultCollection="false" xmlName="type" isReadOnly="false" documentation="Имя класса содержащего методы сервиса">
          <type>
            <externalTypeMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/String" />
          </type>
        </attributeProperty>
      </attributeProperties>
      <elementProperties>
        <elementProperty name="Settings" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="settings" isReadOnly="false" documentation="Основные настройки сервиса">
          <type>
            <configurationElementMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/Settings" />
          </type>
        </elementProperty>
        <elementProperty name="Methods" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="methods" isReadOnly="false" documentation="Список методов, предоставляемых сервисом">
          <type>
            <configurationElementCollectionMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/Methods" />
          </type>
        </elementProperty>
      </elementProperties>
    </configurationElement>
    <configurationElementCollection name="Services" xmlItemName="service" codeGenOptions="Indexer, AddMethod, RemoveMethod, GetItemMethods">
      <itemType>
        <configurationElementMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/Service" />
      </itemType>
    </configurationElementCollection>
    <configurationElement name="Recive">
      <attributeProperties>
        <attributeProperty name="Method" isRequired="true" isKey="false" isDefaultCollection="false" xmlName="method" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="Parameter" isRequired="true" isKey="false" isDefaultCollection="false" xmlName="parameter" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/String" />
          </type>
        </attributeProperty>
      </attributeProperties>
    </configurationElement>
  </configurationElements>
  <propertyValidators>
    <validators />
  </propertyValidators>
</configurationSectionModel>