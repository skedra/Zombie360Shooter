using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class AStar : MonoBehaviour
{
	[SerializeField]
	Renderer m_MapRenderer;

	[SerializeField]
	float m_CellSize = 0.2f;

	[SerializeField]
	float m_AgentRadius = 0.5f;

	public static AStar inst;

	public Vector2 m_Center;

	Vector2 m_Extents;

	bool[,] m_Map;

	List<Vector2> m_OffsetVectors = new List<Vector2>();

	public class PathFindingNode
	{
		public Vector2 m_Pos;
		public Vector2 m_Start;
		public Vector2 m_End;
		public PathFindingNode m_Parent;
		public float m_GDist;
		public float m_HDist;
		public float m_FDist;

		public PathFindingNode(Vector2 pos, Vector2 start, Vector2 end)
		{
			m_Pos = pos;

			m_Start = start;
			m_End = end;

			// Distance from the start
			m_GDist = (m_Pos - start).magnitude;
			// Distance from end node
			m_HDist = (m_Pos - end).magnitude;

			// Total distance
			m_FDist = m_GDist + m_HDist;
		}

		public PathFindingNode(Vector2 pos, PathFindingNode parent)
		{
			m_Pos = pos;

			m_Start = parent.m_Start;
			m_End = parent.m_End;

			m_Parent = parent;

			m_GDist = parent.m_GDist + (pos - parent.m_Pos).magnitude;
			m_HDist = (m_Pos - m_End).magnitude;

			m_FDist = m_GDist + m_HDist;
		}
	}

	void Awake()
	{
		//Lazy singleton
		inst = this;

		// cache the extents on x and z along with the center
		m_Extents = new Vector2(m_MapRenderer.bounds.extents.x, m_MapRenderer.bounds.extents.z);
		m_Center = new Vector2(m_MapRenderer.bounds.center.x, m_MapRenderer.bounds.center.z);

		// calcuate map size based on cell size 
		m_Map =
			new bool[Mathf.CeilToInt(m_Extents.x * 2f / m_CellSize),
				Mathf.CeilToInt(Mathf.CeilToInt(m_Extents.y * 2f / m_CellSize))];

		// fill in the map
		for (float x = -m_Extents.x; x < m_Extents.x; x += m_CellSize)
		{
			for (float y = -m_Extents.y; y < m_Extents.y; y += m_CellSize)
			{
				Ray ray = new Ray(new Vector3(x, 30, y), Vector3.down);
				Vector2 id = GetCellId(new Vector2(x, y));
				m_Map[Mathf.FloorToInt(id.x), Mathf.FloorToInt(id.y)] = !Physics.SphereCast(ray, m_AgentRadius, Mathf.Infinity,
					1 << LayerMask.NameToLayer("NavObstacle"), QueryTriggerInteraction.UseGlobal);
			}
		}


		// create offset vectors for each nav point to use
		m_OffsetVectors.Add(new Vector2(1, 0));
		m_OffsetVectors.Add(new Vector2(0, 1));
		m_OffsetVectors.Add(new Vector2(-1, 0));
		m_OffsetVectors.Add(new Vector2(0, -1));
		m_OffsetVectors.Add(new Vector2(1, 1));
		m_OffsetVectors.Add(new Vector2(-1, 1));
		m_OffsetVectors.Add(new Vector2(1, -1));
		m_OffsetVectors.Add(new Vector2(-1, -1));
	}

	List<PathFindingNode> GetAdjascentNodes(PathFindingNode start, Dictionary<Vector2, PathFindingNode> unusuedNodes)
	{
		// create a new list of nodes
		List<PathFindingNode> adjNodes = new List<PathFindingNode>();

		foreach (var offset in m_OffsetVectors)
		{
			// for each offest value check if the id is within map bounds
			// if it isn't ignore
			Vector2 newId = GetCellId(start.m_Pos + offset * m_CellSize);

			if (newId.x < 0 || newId.x >= m_Map.GetLength(0) || newId.y < 0 || newId.y >= m_Map.GetLength(1))
			{
				continue;
			}

			// if it is and it's not walkable ignore
			if (!IsWalkable(newId))
				continue;

			// if we've already got this point in the list
			if (unusuedNodes.ContainsKey(newId))
			{
				// check if the new distance to start is lower than the previous one
				float tempGDist = start.m_GDist + (offset * m_CellSize).magnitude;
				if (tempGDist < unusuedNodes[newId].m_GDist)
				{
					// if it is replace the old distance with the new one
					unusuedNodes[newId].m_GDist = tempGDist;
					unusuedNodes[newId].m_FDist = tempGDist + unusuedNodes[newId].m_HDist;

					// new node parent from the node that called the method
					unusuedNodes[newId].m_Parent = start;
				}
			}
			else
			{
				// otherwise just create a new node
				PathFindingNode node = new PathFindingNode(start.m_Pos + offset * m_CellSize, start);
				adjNodes.Add(node);
			}

		}

		return adjNodes;
	}

	bool IsWalkable(Vector2 id)
	{
		return m_Map[Mathf.FloorToInt(id.x), Mathf.FloorToInt(id.y)];
	}

	Vector2 GetCellId(Vector2 pos)
	{
		Vector2 id = ((pos + m_Extents) / m_CellSize);
		return new Vector2(Mathf.FloorToInt(id.x), Mathf.FloorToInt(id.y));
	}

	public List<PathFindingNode> GetPath(Vector2 start, Vector2 end)
	{
		// starting path
		PathFindingNode path = new PathFindingNode(start, start, end);
		path.m_Parent = null;

		// unused nodes for finidng the node that is best suited as next path test
		Dictionary<Vector2, PathFindingNode> unusedNodes = new Dictionary<Vector2, PathFindingNode>();

		// closed nodes  for ones which have already been used not to clutter the list
		Dictionary<Vector2, PathFindingNode> closedNodes = new Dictionary<Vector2, PathFindingNode>();

		bool found = false;

		// for safety add a exit loop counter
		int count = 0;
		while (!found && count < 2500)
		{
			// get adjasent nodes
			List<PathFindingNode> newNodes = GetAdjascentNodes(path, unusedNodes);

			// if they are not within the old nodes closed nodes
			newNodes = newNodes.Where(x => !closedNodes.ContainsKey(GetCellId(x.m_Pos))).ToList();

			// add them to the unused nodes
			foreach (PathFindingNode n in newNodes)
			{
				unusedNodes.Add(GetCellId(n.m_Pos), n);
			}

			// order the nodes by their total dist first, then distance to the end and finally distance to the start
			unusedNodes = unusedNodes.OrderBy(x => x.Value.m_FDist).ThenBy(z => z.Value.m_HDist).ThenBy(y => y.Value.m_GDist).ToDictionary(t => t.Key, t => t.Value);

			// if we still got nodes
			if (unusedNodes.Count > 0)
			{
				// setup the best suited as new path
				path = unusedNodes.Values.ElementAt(0);

				// if reached the target cell
				if ((GetCellId(path.m_Pos) - GetCellId(end)).magnitude == 0)
				{
					found = true;
				}
				else
				{
					// otherwise add the new node to the used nodes and remove from unused
					closedNodes.Add(GetCellId(path.m_Pos), path);
					unusedNodes.Remove(GetCellId(path.m_Pos));
				}
			}
			else
			{
				// otherwise fail
				path = null;
				break;
			}

			count++;
		}

		List<PathFindingNode> points = new List<PathFindingNode>();

		// list path if exist in reversed order
		while (path != null)
		{
			points.Insert(0, path);
			path = path.m_Parent;
		}

		return points;
	}


	// DEBUG map view
	//void OnDrawGizmos()
	//{
	//	if (UnityEditor.EditorApplication.isPlaying)
	//	{
	//		for (float x = -m_Extents.x; x < m_Extents.x; x += m_PointOffset)
	//		{
	//			for (float y = -m_Extents.y; y < m_Extents.y; y += m_PointOffset)
	//			{
	//				Vector2 id = GetCellId(new Vector2(x, y));
	//				Gizmos.color = IsWalkable(id) ? Color.green : Color.red;
	//				Gizmos.DrawCube(new Vector3(x, 0, y) + new Vector3(m_Center.x, 0, m_Center.y), Vector3.one * m_PointOffset);
	//			}
	//		}
	//	}
	//}
}
