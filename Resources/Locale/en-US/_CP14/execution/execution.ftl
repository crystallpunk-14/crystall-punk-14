# All the below localisation strings have access to the following variables
# attacker (the person committing the execution)
# victim (the person being executed)
# weapon (the weapon used for the execution)

blunt-execution-popup-melee-initial-internal = You ready {THE($weapon)} to execute {THE($victim)}.
blunt-execution-popup-melee-initial-external = { CAPITALIZE(THE($attacker)) } readies {POSS-ADJ($attacker)} {$weapon} to execute {THE($victim)}.
blunt-execution-popup-melee-complete-internal = You executed {THE($victim)}!
blunt-execution-popup-melee-complete-external = { CAPITALIZE(THE($attacker)) } executed {THE($victim)}!

blunt-execution-popup-self-initial-internal = You ready {THE($weapon)} against your own head.
blunt-execution-popup-self-initial-external = { CAPITALIZE(THE($attacker)) } readies {POSS-ADJ($attacker)} {$weapon} against their own head.
blunt-execution-popup-self-complete-internal = You killed yourself!
blunt-execution-popup-self-complete-external = { CAPITALIZE(THE($attacker)) } kills themself!
