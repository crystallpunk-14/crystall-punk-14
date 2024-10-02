cp14-ritual-trigger-header = Переход в этот узел может состояться:

#

cp14-ritual-trigger-timer-stable = Автоматически через {$time} секунд.
cp14-ritual-trigger-timer-unstable = Автоматически в течении от {$min} до {$max} секунд.

cp14-ritual-trigger-voice = После произнесения заклинания "{$phrase}" в радиусе 
    { $count ->
        [1] {$range} метров
        *[other] {$range} метров, как минимум {$count} разными сущностями одновременно
    }

cp14-ritual-trigger-voice-limits = В результате ошибок произнесения заклинаний.