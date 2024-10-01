cp14-ritual-effect-header = Переход в {$node} вызовет следующие эффекты:

cp14-ritual-filter-humanoid = с свойствами гуманоидных существ
cp14-ritual-filter-energy-storage = с способностью хранить магическую энергию

cp14-ritual-range =  В радиусе {$range},

##

cp14-ritual-effect-apply-effect = 
    у { $count ->
        [1] ближайшей сущности
        *[other] ближайших сущностей
    }


cp14-ritual-effect-spawn-entity = Воплощает {$name} в реальность
    { $count ->
        [1].
        *[other] в количестве {$count} единиц.
    }

cp14-ritual-effect-consume-resource =
    поглощает { $count ->
        [1] {$name}
        *[other] {$name} в количестве {$count} единиц.
    }

cp14-ritual-effect-stability-add = Стабилизирует ритуал на [color="#34eb89"]{$count}%[/color]
cp14-ritual-effect-stability-minus = Дестабилизирует ритуал на [color="#eb4034"]{$count}%[/color]