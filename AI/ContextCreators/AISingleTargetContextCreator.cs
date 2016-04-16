using System.Collections.Generic;

class AISingleTargetContextCreator : AIDecisionContextCreator {

    public float radius = 40f;
    public string faction = "All";

    public override Context[] GetContexts(Entity agent) {
        List<Entity> hostiles = EntityManager.Instance.NearestHostiles(agent, radius);
        var retn = new Context[hostiles.Count];
        for (int i = 0; i < retn.Length; i++) {
            var ctx = new Context();
            ctx.entity = agent;
            ctx["target"] = hostiles[i];
            retn[i] = ctx;
        }

        return retn;
    }
}

