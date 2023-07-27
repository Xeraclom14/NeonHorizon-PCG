using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

public class WFCPathfinding : MonoBehaviour {

	public float nodeSize = 2;
	public Transform seeker;
	
	[SerializeField]
	public NodeGrid[] grids;

	void Update() {
		Stopwatch sw = new Stopwatch();

		sw.Start();
		FindPath(seeker.position,GameManager.playerScript.transform.position);
		sw.Stop();

		//UnityEngine.Debug.Log(sw.Elapsed.TotalSeconds * 1000f);
	}

	public void UpdateGrids(int floors, Vector3 firstFloorCenter, Vector2 size, float scale)
	{
		Vector2 worldSize;

		worldSize.x = (size.x - 3) * 4 * scale;
		worldSize.y = (size.y - 3) * 4 * scale;

		grids = new NodeGrid[floors];

		for(int i = 0; i < floors; i++)
		{
			grids[i] = new NodeGrid(worldSize, new Vector3(firstFloorCenter.x, firstFloorCenter.y + i * 4 * scale, firstFloorCenter.z), nodeSize);
		}
	}

	NodeGrid GetNearestGrid(Vector3 pos)
	{
		NodeGrid ret = null;
		float nearest = Mathf.Infinity;

		if(grids != null) foreach(NodeGrid grid in grids)
		{
			float dif = Mathf.Abs(pos.y - grid.center.y);
			if (dif < nearest)
			{
				ret = grid;
				nearest = dif;
			}
		}

		return ret;
	}

	void FindPath(Vector3 startPos, Vector3 targetPos) {

		NodeGrid grid = GetNearestGrid(targetPos);

		if (grid == null) return;

		Node startNode = grid.NodeFromWorldPoint(startPos);
		Node targetNode = grid.NodeFromWorldPoint(targetPos);

		if (!startNode.walkable || !targetNode.walkable) return;

		Heap<Node> openSet = new Heap<Node>(grid.MaxSize);
		HashSet<Node> closedSet = new HashSet<Node>();
		openSet.Add(startNode);

		while (openSet.Count > 0) {
			Node currentNode = openSet.RemoveFirst();
			closedSet.Add(currentNode);

			if (currentNode == targetNode) {

				List<Node> path = new List<Node>();
				Node curr = targetNode;

				while (curr != startNode)
				{
					path.Add(curr);
					curr = curr.parent;
				}
				path.Add(startNode);

				path.Reverse();

				grid.path = path;

				return;
			}

			foreach (Node neighbour in grid.GetNeighbours(currentNode)) {
				if (!neighbour.walkable || closedSet.Contains(neighbour)) {
					continue;
				}

				int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
				if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour)) {
					neighbour.gCost = newMovementCostToNeighbour;
					neighbour.hCost = GetDistance(neighbour, targetNode);
					neighbour.parent = currentNode;

					if (!openSet.Contains(neighbour))
						openSet.Add(neighbour);
					else {
						openSet.UpdateItem(neighbour);
					}
				}
			}
		}
	}

	int GetDistance(Node nodeA, Node nodeB) {
		int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
		int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

		if (dstX > dstY)
			return 14*dstY + 10* (dstX-dstY);
		return 14*dstX + 10 * (dstY-dstX);
	}

	private void OnDrawGizmos()
	{
		if(grids != null) foreach (NodeGrid grid in grids)
		{
			if (grid == null) return;

			Gizmos.color = Color.white;
			Gizmos.DrawWireCube(grid.center, new Vector3(grid.gridWorldSize.x, 0, grid.gridWorldSize.y));

			if (grid.onlyDisplayPathGizmos)
			{
				if (grid.path != null)
				{
					foreach (Node n in grid.path)
					{
						Gizmos.color = Color.green;
						Gizmos.DrawWireCube(new Vector3(n.worldPosition.x, grid.center.y + .1f, n.worldPosition.z), new Vector3(grid.nodeSize - .1f, 0f, grid.nodeSize - .1f));
					}
				}
			}
			else
			{
				if(grid.grid != null) foreach (Node n in grid.grid)
				{
					if (!n.walkable) continue;

					if (grid.path != null)
						if (grid.path.Contains(n))
							Gizmos.color = Color.green;
						else
							Gizmos.color = Color.white;

					Gizmos.DrawWireCube(new Vector3(n.worldPosition.x, grid.center.y + 0.05f, n.worldPosition.z), new Vector3(nodeSize - .2f, 0f, nodeSize - .2f));
				}
			}
		}
	}
}
