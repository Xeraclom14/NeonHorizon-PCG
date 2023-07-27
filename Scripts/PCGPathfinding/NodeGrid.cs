using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class NodeGrid {

	public Vector3 center;
	public bool onlyDisplayPathGizmos = true;
	public Vector2 gridWorldSize;
	public float nodeSize;
	public Node[,] grid;
	public List<Node> path;

	public int gridSizeX, gridSizeY;

	public NodeGrid(Vector2 gridWorldSize, Vector3 center, float nodeSize) {
		this.gridWorldSize = gridWorldSize;
		this.center = center;
		this.nodeSize = nodeSize;

		gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeSize);
		gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeSize);

		CreateGrid();
	}

	public int MaxSize {
		get {
			return gridSizeX * gridSizeY;
		}
	}

	void CreateGrid() {
		grid = new Node[gridSizeX, gridSizeY];
		Vector3 worldBottomLeft = center - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;

		for (int x = 0; x < gridSizeX; x++) {
			for (int y = 0; y < gridSizeY; y++) {
				Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeSize + nodeSize / 2f) + Vector3.forward * (y * nodeSize + nodeSize / 2);
				bool walkable = Physics.Raycast(worldPoint + Vector3.up, Vector3.down, 2f);
				grid[x, y] = new Node(walkable, worldPoint, x, y);
			}
		}
	}

	public List<Node> GetNeighbours(Node node) {
		List<Node> neighbours = new List<Node>();

		for (int x = -1; x <= 1; x++) {
			for (int y = -1; y <= 1; y++) {
				if (x == 0 && y == 0)
					continue;

				int checkX = node.gridX + x;
				int checkY = node.gridY + y;

				if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY) {
					neighbours.Add(grid[checkX, checkY]);
				}
			}
		}

		return neighbours;
	}

	public Node NodeFromWorldPoint(Vector3 worldPosition) {
		float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
		float percentY = (worldPosition.z + gridWorldSize.y / 2) / gridWorldSize.y;
		percentX = Mathf.Clamp01(percentX);
		percentY = Mathf.Clamp01(percentY);

		int x = Mathf.Clamp(Mathf.FloorToInt(gridSizeX * percentX), 0, gridSizeX - 1);
		int y = Mathf.Clamp(Mathf.FloorToInt(gridSizeY * percentY), 0, gridSizeY - 1);

		return grid[x, y];
	}

}