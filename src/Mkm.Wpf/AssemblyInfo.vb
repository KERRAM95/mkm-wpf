Imports System.Windows
Imports System.Windows.Markup

' Les styles par défaut des contrôles sont dans Themes/Generic.xaml de cet assembly,
' sans dictionnaire propre à un thème de Windows (Aero, Luna...).
<Assembly: ThemeInfo(ResourceDictionaryLocation.None, ResourceDictionaryLocation.SourceAssembly)>

' Un seul espace de noms XAML pour toute la bibliothèque : xmlns:mkm="urn:mkm-wpf".
' Chaque espace de noms public de l'assembly y est associé par sa propre ligne.
<Assembly: XmlnsPrefix("urn:mkm-wpf", "mkm")>
<Assembly: XmlnsDefinition("urn:mkm-wpf", "Mkm.Wpf.Controls")>
