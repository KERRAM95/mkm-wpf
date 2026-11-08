Imports System.Globalization
Imports System.Windows.Data

Namespace Mkm.Wpf.Controls

    ''' <summary>Vrai quand la valeur liée existe (un ancêtre trouvé, par exemple) : pour un déclencheur de style.</summary>
    Friend NotInheritable Class EstPresent
        Implements IValueConverter

        Public Shared ReadOnly Instance As New EstPresent()

        Public Function Convert(value As Object, targetType As System.Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
            Return value IsNot Nothing
        End Function

        Public Function ConvertBack(value As Object, targetType As System.Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
            Return Binding.DoNothing
        End Function

    End Class

End Namespace
