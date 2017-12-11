﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace CPF_experiment
{
    class AStarMDD
    {
        MDD[] problem;
        Dictionary<MDDStep, MDDStep> closedList;
        Run runner;
        BinaryHeap openList; // TODO: Is A*MDD a best-first-search? Is it safe to use OpenList instead?
        public int expanded;
        public int generated;
        public int conflictAvoidanceViolations;
        HashSet<TimedMove> ID_CAT;
        HashSet_U<TimedMove> CBS_CAT;

        public AStarMDD(MDD[] problem, Run runner, HashSet<TimedMove> conflicts, HashSet_U<TimedMove> CBS_CAT)
        {
            this.expanded = 0;
            this.generated = 0;
            MDDStep root;
            this.problem = problem;
            this.runner = runner;
            this.ID_CAT = conflicts;
            this.CBS_CAT = CBS_CAT;
            this.closedList = new Dictionary<MDDStep, MDDStep>();
            this.openList = new BinaryHeap();
            MDDNode[] sRoot = new MDDNode[problem.Length];
            for (int i = 0; i < problem.Length; i++)
            {
                sRoot[i] = problem[i].levels[0].First.Value;
                sRoot[i].legal = true;
            }
            root = new MDDStep(sRoot, null);
            openList.Add(root);
            // Not adding it automatically to the closed list here?
            conflictAvoidanceViolations = 0;
        }
       
        public LinkedList<Move>[] Solve()
        {
            MDDStep currentNode;
            ExpandedNode toExpand = new ExpandedNode();

            while (openList.Count > 0)
            {
                 if (runner.ElapsedMilliseconds() > Constants.MAX_TIME)
                {
                    return null;
                }
                 currentNode = (MDDStep)openList.Remove();
                // Check if node is the goal
                if (this.GoalTest(currentNode))
                {
                    conflictAvoidanceViolations=currentNode.conflicts;
                    return GetAnswer(currentNode);
                }

                // Expand
                expanded++;
                toExpand.setup(currentNode);
                Expand(toExpand);
            }
            return null;
        }

        public void Expand(ExpandedNode currentNode)
        {
            MDDStep child = currentNode.getNextChild();
            while (child != null)
            {
                if (IsLegalMove(child))
                {
                        child.conflicts = currentNode.parent.conflicts;
                        child.setConflicts(ID_CAT, CBS_CAT);

                    if (this.closedList.ContainsKey(child) == true)
                    {
                        MDDStep inClosedList = this.closedList[child];

                        if (inClosedList.conflicts > child.conflicts)
                        {
                            closedList.Remove(inClosedList);
                            openList.Remove(inClosedList);
                        }
                    }
                    if (this.closedList.ContainsKey(child) == false)
                    {
                        this.openList.Add(child);
                        this.closedList.Add(child, child);
                        generated++;
                    }
                }
                child = currentNode.getNextChild();
            }
        }

        /// <summary>
        /// Not used.
        /// </summary>
        public void ClearIllegal()
        {
            foreach (MDD mdd in problem)
                foreach (LinkedList<MDDNode> level in mdd.levels)
                    foreach (MDDNode node in level)
                        if (node.legal == false)
                            node.delete();
        }

        /// <summary>
        /// Not used.
        /// </summary>
        public void ResetIllegal()
        {
            foreach (MDD mdd in problem)
                for (int i = 1; i < mdd.levels.Length; i++)
                    foreach (MDDNode node in mdd.levels[i])
                            node.legal = false;
        }

        public int GetGenerated() { return this.generated; }
        
        public int GetExpanded() { return this.expanded; }
        
        private bool GoalTest(MDDStep toCheck)
        {
            if (toCheck.getDepth() == problem[0].levels.Length - 1)
                return true;
            return false;
        }

        private LinkedList<Move>[] GetAnswer(MDDStep finish)
        {
            if (finish == null)
                return new LinkedList<Move>[1];
            LinkedList<Move>[] ans = new LinkedList<Move>[problem.Length];
            Move.Direction direction;
            for (int i = 0; i < ans.Length; i++)
                ans[i] = new LinkedList<Move>();
            MDDStep current = finish;
            while (current.prevStep != null)
            {
                for (int i = 0; i < problem.Length; i++)
                {
                    direction = Move.getDirection(current.allSteps[i].getX(), current.allSteps[i].getY(), current.prevStep.allSteps[i].getX(), current.prevStep.allSteps[i].getY());
                    ans[i].AddFirst(new Move(current.allSteps[i].getX(), current.allSteps[i].getY(), direction));
                }
                current = current.prevStep;
            }
            for (int i = 0; i < problem.Length; i++)
            {
                ans[i].AddFirst(new Move(current.allSteps[i].getX(), current.allSteps[i].getY(), 0));
            }
            return ans;
        }
        
        private bool CheckIfLegal(MDDNode from1, MDDNode to1, MDDNode from2, MDDNode to2)
        {
            if (to1.getX() == to2.getX() && to1.getY() == to2.getY())
                return false;
            if (to1.getX() == from2.getX() && from1.getX() == to2.getX() && to1.getY() == from2.getY() && from1.getY() == to2.getY())
                return false;
            return true;
        }
        
        private bool IsLegalMove(MDDStep to)
        {
            if (to == null)
                return false;
            if (to.prevStep == null)
                return true;
            for (int i = 0; i < problem.Length; i++)
            {
                for (int j = i+1; j < to.allSteps.Length; j++)
                {
                    if (CheckIfLegal(to.prevStep.allSteps[i], to.allSteps[i], to.prevStep.allSteps[j], to.allSteps[j]) == false)
                        return false;
                }
            }
            return true;
        }
    }


    class MDDStep : IComparable<IBinaryHeapItem>, IBinaryHeapItem
    {
        public MDDNode[] allSteps;
        public MDDStep prevStep;
        public int conflicts;
        int binaryHeapIndex;

        public MDDStep(MDDNode[] allSteps, MDDStep prevStep)
        { 
            this.allSteps = allSteps;
            this.prevStep = prevStep;
        }

        public MDDStep(MDDStep cpy)
        {
            this.allSteps = new MDDNode[cpy.allSteps.Length];
            for (int i = 0; i < allSteps.Length; i++)
            {
                this.allSteps[i] = cpy.allSteps[i];
            }
            this.prevStep = cpy.prevStep;
        }

        /// <summary>
        /// Only compares the steps.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            MDDStep comp = (MDDStep)obj;
            return this.allSteps.SequenceEqual<MDDNode>(comp.allSteps);
        }

        /// <summary>
        /// Only uses the steps
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int code = 0;
                for (int i = 0; i < allSteps.Length; i++)
                {
                    code += allSteps[i].GetHashCode() * Constants.PRIMES_FOR_HASHING[i % Constants.PRIMES_FOR_HASHING.Length];
                }
                return code;
            }
        }

        public int getDepth() { return allSteps[0].move.time; }

        public void setConflicts(HashSet<TimedMove> ID_CAT, HashSet_U<TimedMove> CBS_CAT)
        {
            TimedMove m2 = new TimedMove();
            if (this.prevStep == null)
                return;
            for (int i = 0; i < allSteps.Length; i++)
            {
                    m2.setup(allSteps[i].getX(), allSteps[i].getY(), Move.Direction.NO_DIRECTION, getDepth());
                    if (ID_CAT != null && ID_CAT.Contains(m2))
                        conflicts++;
                    if (CBS_CAT != null && CBS_CAT.Contains(m2))
                        conflicts++;
                    m2.direction = Move.getDirection(allSteps[i].getX(), allSteps[i].getY(), prevStep.allSteps[i].getX(), prevStep.allSteps[i].getY());
                    m2.setOppositeMove();
                    if (ID_CAT != null && ID_CAT.Contains(m2))
                        conflicts++;
                    if (CBS_CAT != null && CBS_CAT.Contains(m2))
                        conflicts++;
            }
        }

        /// <summary>
        /// BH_Item implementation
        /// </summary>
        /// <returns></returns>
        public int GetIndexInHeap() { return binaryHeapIndex; }
        
        /// <summary>
        /// BH_Item implementation
        /// </summary>
        /// <returns></returns>
        public void SetIndexInHeap(int index) { binaryHeapIndex = index; }

        public int CompareTo(IBinaryHeapItem other)
        {
            MDDStep that = (MDDStep)other;
            if (this.conflicts  < that.conflicts)
                return -1;
            if (this.conflicts > that.conflicts)
                return 1;

            if (this.getDepth() > that.getDepth())
                return -1;
            if (this.getDepth() < that.getDepth())
                return 1;

            return 0;
        }
    }

    class ExpandedNode
    {
        public MDDStep parent;
        int[] chosenChild;

        public ExpandedNode() { }

        public ExpandedNode(MDDStep parent)
        {
            this.parent = parent;
            chosenChild = new int[parent.allSteps.Length];
            foreach (MDDNode node in parent.allSteps)
            {
                if (node.children.Count == 0)
                {
                    chosenChild[0] = -1;
                    break;
                }
            }
        }

        public void setup(MDDStep parent)
        {
            this.parent = parent;
            chosenChild = new int[parent.allSteps.Length];
            foreach (MDDNode node in parent.allSteps)
            {
                if (node.children.Count == 0)
                {
                    chosenChild[0] = -1;
                    break;
                }
            }
        }

        public MDDStep getNextChild()
        {
            if (chosenChild[0] == -1)
                return null;
            MDDNode[] ans=new MDDNode[parent.allSteps.Length];
            for (int i = 0; i < ans.Length; i++)
            {
                ans[i] = parent.allSteps[i].children.ElementAt(chosenChild[i]);
            }
            setNextChild(chosenChild.Length - 1);
            return new MDDStep(ans, parent);
        }

        private void setNextChild(int agentNum)
        {
            if (agentNum == -1)
                chosenChild[0] = -1;
            else if (chosenChild[agentNum] < parent.allSteps[agentNum].children.Count - 1)
                chosenChild[agentNum]++;
            else
            {
                chosenChild[agentNum] = 0;
                setNextChild(agentNum - 1);
            }
        }
    }
}
