cp14-ritual-trigger-header = Известные точки взаимодействия с узлом:
cp14-ritual-trigger-none = [color=#992323]отсутствуют[/color]

#

cp14-ritual-trigger-timer-stable = Автоматический переход в [color=#e6a132]{$node}[/color] ожидается через {$time} секунд.
cp14-ritual-trigger-timer-unstable = Автоматический переход в [color=#e6a132]{$node}[/color] ожидается в течении от {$min} до {$max} секунд.

cp14-ritual-trigger-voice = Произнесение заклинания "{$phrase}" в радиусе {$range}
    { $count ->
        [1] метров
        *[other] как минимум {$count} разными сущностями одновременно
    } вызовет переход в [color=#e6a132]{$node}[/color].

cp14-ritual-trigger-voice-limits = Неправильное произношение заклинания может привести ритуал в [color=#e6a132]{$node}[/color]!