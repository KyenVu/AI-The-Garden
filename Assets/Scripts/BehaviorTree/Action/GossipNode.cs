using UnityEngine;

public class GossipNode : Node
{
    private AgentBlackBoard bb;
    private AgentAI selfAgent;
    private float chatDuration = 4f;
    private float chatTimer = 0f;

    public GossipNode(AgentBlackBoard blackBoard, AgentAI self)
    {
        this.bb = blackBoard;
        this.selfAgent = self;
    }

    public override NodeState Evaluate()
    {
        if (bb.gossipCooldownTimer > 0) bb.gossipCooldownTimer -= Time.deltaTime;

        // 1. IF WE ARE CURRENTLY CHATTING
        if (bb.chatPartner != null)
        {
            // Partner abandoned the chat (e.g., got hungry)
            if (Vector2.Distance(bb.mover.transform.position, bb.chatPartner.bb.mover.transform.position) > bb.communicationRadius * 1.5f)
            {
                bb.ui?.SetState("Partner left...");
                bb.chatPartner.bb.chatPartner = null; // Free the partner
                bb.chatPartner = null;
                chatTimer = 0f;
                return _state = NodeState.Failure;
            }

            bb.ui?.SetState("Sharing Info...");
            chatTimer += Time.deltaTime;

            if (chatTimer >= chatDuration)
            {
                // Trade information both ways!
                ShareKnowledgeWith(bb.chatPartner.bb);

                // Put BOTH agents on cooldown and free them simultaneously
                bb.chatPartner.bb.gossipCooldownTimer = 10f;
                bb.chatPartner.bb.chatPartner = null;

                bb.gossipCooldownTimer = 10f;
                bb.chatPartner = null;
                chatTimer = 0f;
                return _state = NodeState.Success;
            }

            return _state = NodeState.Running;
        }

        // 2. LOOKING FOR A CHAT PARTNER
        if (bb.gossipCooldownTimer > 0) return _state = NodeState.Failure;

        Collider2D[] hits = Physics2D.OverlapCircleAll(bb.mover.transform.position, bb.communicationRadius, bb.agentLayer);

        // ADD THESE DEBUG LINES:
        if (hits.Length > 0) Debug.Log($"{selfAgent.name} sees {hits.Length} objects in range!");

        foreach (Collider2D hit in hits)
        {
            AgentAI otherAgent = hit.GetComponent<AgentAI>();

            if (otherAgent != null && otherAgent != selfAgent)
            {
                Debug.Log($"{selfAgent.name} found {otherAgent.name}. Checking if they can talk...");

                // Are they free to chat?
                if (otherAgent.bb.chatPartner == null && otherAgent.bb.gossipCooldownTimer <= 0)
                {
                    if (HasNewInfo(bb, otherAgent.bb))
                    {
                        Debug.Log("NEW INFO FOUND! STARTING CHAT!"); // You want to see this!
                        bb.chatPartner = otherAgent;
                        // ... rest of the code
                    }
                    else
                    {
                        Debug.Log($"They have no new info for each other. Skipping.");
                    }
                }
                else
                {
                    Debug.Log($"{otherAgent.name} is busy or on cooldown.");
                }
            }
        }

        return _state = NodeState.Failure;
    }

    private bool HasNewInfo(AgentBlackBoard a, AgentBlackBoard b)
    {
        // Clean nulls first
        a.knownFoods.RemoveAll(x => x == null); a.knownTrees.RemoveAll(x => x == null); a.knownWaters.RemoveAll(x => x == null);
        b.knownFoods.RemoveAll(x => x == null); b.knownTrees.RemoveAll(x => x == null); b.knownWaters.RemoveAll(x => x == null);

        // Does A have something B doesn't?
        foreach (Food f in a.knownFoods) if (!b.knownFoods.Contains(f)) return true;
        foreach (Tree t in a.knownTrees) if (!b.knownTrees.Contains(t)) return true;
        foreach (WaterSource w in a.knownWaters) if (!b.knownWaters.Contains(w)) return true;

        // Does B have something A doesn't?
        foreach (Food f in b.knownFoods) if (!a.knownFoods.Contains(f)) return true;
        foreach (Tree t in b.knownTrees) if (!a.knownTrees.Contains(t)) return true;
        foreach (WaterSource w in b.knownWaters) if (!a.knownWaters.Contains(w)) return true;

        return false; // They know exactly the same things! No need to chat.
    }

    private void ShareKnowledgeWith(AgentBlackBoard otherBB)
    {
        // Agent A learns from Agent B
        foreach (Food f in otherBB.knownFoods) if (!bb.knownFoods.Contains(f)) bb.knownFoods.Add(f);
        foreach (Tree t in otherBB.knownTrees) if (!bb.knownTrees.Contains(t)) bb.knownTrees.Add(t);
        foreach (WaterSource w in otherBB.knownWaters) if (!bb.knownWaters.Contains(w)) bb.knownWaters.Add(w);

        // Agent B learns from Agent A instantly
        foreach (Food f in bb.knownFoods) if (!otherBB.knownFoods.Contains(f)) otherBB.knownFoods.Add(f);
        foreach (Tree t in bb.knownTrees) if (!otherBB.knownTrees.Contains(t)) otherBB.knownTrees.Add(t);
        foreach (WaterSource w in bb.knownWaters) if (!otherBB.knownWaters.Contains(w)) otherBB.knownWaters.Add(w);

        Debug.Log($"{selfAgent.gameObject.name} and {otherBB.mover.gameObject.name} traded info!");
    }
}