Imports NUnit.Framework

' Un seul utilisateur devant l'écran : les scénarios se suivent, jamais deux à la fois.
<Assembly: NonParallelizable>
<Assembly: LevelOfParallelism(1)>
