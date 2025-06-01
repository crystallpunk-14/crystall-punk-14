using Content.Shared._CP14.Input;
using Robust.Shared.Input;

namespace Content.Client._CP14.Input
{
    public static class CP14ContentContexts
    {
        public static void SetupContexts(IInputContextContainer contexts)
        {
            var human = contexts.GetContext("human");
            human.AddFunction(CP14ContentKeyFunctions.OpenBelt2);
            human.AddFunction(CP14ContentKeyFunctions.SmartEquipBelt2);
            human.AddFunction(CP14ContentKeyFunctions.CP14OpenSkillMenu);
        }
    }
}
