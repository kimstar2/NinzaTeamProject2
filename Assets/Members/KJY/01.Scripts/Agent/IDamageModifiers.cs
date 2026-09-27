namespace Members.KJY._01.Scripts.Agent
{
    // Combat queries multipliers; event storage and duration rules stay outside combat.
    public interface IDamageModifiers
    {
        float GetOutgoingMultiplier(AgentDataSO attacker);
        float GetIncomingMultiplier(AgentDataSO target);
    }
}
