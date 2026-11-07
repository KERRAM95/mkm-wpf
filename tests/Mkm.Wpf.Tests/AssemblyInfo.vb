Imports System.Threading
Imports NUnit.Framework

' WPF exige un thread STA : tous les tests de l'assembly y tournent.
<Assembly: Apartment(ApartmentState.STA)>
