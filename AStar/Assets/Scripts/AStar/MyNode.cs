using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pathing
{
	class MyNode : IAStarNode
	{
		// 0 desert 1 forest 2 grass 3 mountain 4 water
		// 5 days   3 days   1 day   10 days    positive infinity
		public int typeIndex;
		public float[] typeCost = new float[] {5.0f, 3.0f, 1.0f, 10.0f, float.PositiveInfinity };

		// offset system
		public int x;
		public int y;
		public int globalIndex;
		public string typeName;

		public MyNode(string typeName, int typeIndex, int x, int y, int globalIndex, IEnumerable<IAStarNode> Neighbours = null)
		{
			this.typeName = typeName;
			this.typeIndex = typeIndex;
			this.globalIndex = globalIndex;
			this.x = x;
			this.y = y;
			this.Neighbours = Neighbours == null ? new List<IAStarNode>() : Neighbours;
		}

		public MyNode(IEnumerable<IAStarNode> Neighbours = null)
		{
			this.Neighbours = Neighbours;
			
		}

		public IEnumerable<IAStarNode> Neighbours
		{
			get;
			set;
		}

		// this function should calculate the exact cost of travelling from this node to "neighbour".
		// when the A* algorithm calls this function, the neighbour parameter is guaranteed to be one of the nodes in 'Neighbours'.
		//
		// 'cost' could be distance, time, anything countable, where smaller is considered better by the algorithm
		public float CostTo(IAStarNode neighbour)
		{
			if (!this.Neighbours.Contains(neighbour))
            {
				// double check these two nodes are not neighbours
				// return float.PositiveInfinity;
				return -1.0f;
            }
			
			MyNode myNode = (MyNode)neighbour;
			return typeCost[myNode.typeIndex];

		}

		// this function should estimate the distance to travel from this node to "goal". goal may be
		// any node in the graph, so there is no guarantee it is a direct neighbour. The better the estimation
		// the faster the AStar algorithm will find the optimal route. Be careful however, that the cost of calculating
		// this estimate doesn't outweigh any benefits for the AStar search.
		//
		// this estimation could be distance, time, anything countable, where smaller is considered better by the algorithm
		// the estimate needs to 'consistent' (also know as 'monotone')
		public float EstimatedCostTo(IAStarNode goal)
		{
			MyNode myNode = (MyNode)goal;
			
			return manhattanDistance(myNode.x, myNode.y, this.x, this.y);

		}

		private float manhattanDistance(int x1, int y1, int x2, int y2)
        {
			return Math.Abs(x1 - x2) + Math.Abs(y1 - y2);
		}

	}
}
