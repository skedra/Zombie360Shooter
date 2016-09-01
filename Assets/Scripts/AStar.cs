using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class AStar : MonoBehaviour
{
	[SerializeField]
	Renderer m_MapRenderer;

	[SerializeField]
	float m_PointOffset = 0.2f;

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
		public bool m_Closed = false;

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
		m_Extents = new Vector2(m_MapRenderer.bounds.extents.x, m_MapRenderer.bounds.extents.z);
		m_Center = new Vector2(m_MapRenderer.bounds.center.x, m_MapRenderer.bounds.center.z);
		m_Map =
			new bool[Mathf.CeilToInt(m_Extents.x * 2f / m_PointOffset),
				Mathf.CeilToInt(Mathf.CeilToInt(m_Extents.y * 2f / m_PointOffset))];

		for (float x = -m_Extents.x; x < m_Extents.x; x += m_PointOffset)
		{
			for (float y = -m_Extents.y; y < m_Extents.y; y += m_PointOffset)
			{
				Ray ray = new Ray(new Vector3(x, 30, y), Vector3.down);
				Vector2 id = GetCellId(new Vector2(x, y));
				m_Map[Mathf.FloorToInt(id.x), Mathf.FloorToInt(id.y)] = !Physics.SphereCast(ray, m_AgentRadius, Mathf.Infinity,
					1 << LayerMask.NameToLayer("NavObstacle"), QueryTriggerInteraction.UseGlobal);
			}
		}

		m_OffsetVectors.Add(new Vector2(1, 0));
		m_OffsetVectors.Add(new Vector2(0, 1));
		m_OffsetVectors.Add(new Vector2(-1, 0));
		m_OffsetVectors.Add(new Vector2(0, -1));
		m_OffsetVectors.Add(new Vector2(1, 1));
		m_OffsetVectors.Add(new Vector2(-1, 1));
		m_OffsetVectors.Add(new Vector2(1, -1));
		m_OffsetVectors.Add(new Vector2(-1, -1));
	}

	// Update is called once per frame
	void Update()
	{

	}

	List<PathFindingNode> GetAdjascentNodes(PathFindingNode start, Dictionary<Vector2, PathFindingNode> unusuedNodes)
	{
		List<PathFindingNode> adjNodes = new List<PathFindingNode>();

		foreach (var offset in m_OffsetVectors)
		{
			Vector2 newId = GetCellId(start.m_Pos + offset * m_PointOffset);

			if (newId.x < 0 || newId.x >= m_Map.GetLength(0) || newId.y < 0 || newId.y >= m_Map.GetLength(1))
			{
				continue;
			}
			if (!IsWalkable(newId))
				continue;

			if (unusuedNodes.ContainsKey(newId))
			{
				float tempGDist = start.m_GDist + (offset * m_PointOffset).magnitude;
				if (tempGDist < unusuedNodes[newId].m_GDist)
				{
					unusuedNodes[newId].m_GDist = tempGDist;
					unusuedNodes[newId].m_FDist = tempGDist + unusuedNodes[newId].m_HDist;
					unusuedNodes[newId].m_Parent = start;
				}
			}
			else
			{
				PathFindingNode node = new PathFindingNode(start.m_Pos + offset * m_PointOffset, start);
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
		Vector2 id = ((pos + m_Extents) / m_PointOffset);
		return new Vector2(Mathf.FloorToInt(id.x), Mathf.FloorToInt(id.y));
	}

	public List<PathFindingNode> GetPath(Vector2 start, Vector2 end)
	{
		PathFindingNode path = new PathFindingNode(start, start, end);
		path.m_Parent = null;
		Dictionary<Vector2, PathFindingNode> unusedNodes = new Dictionary<Vector2, PathFindingNode>();
		Dictionary<Vector2, PathFindingNode> closedNodes = new Dictionary<Vector2, PathFindingNode>();

		bool found = false;
		int count = 0;
		while (!found && count < 1200)
		{
			List<PathFindingNode> newNodes = GetAdjascentNodes(path, unusedNodes);

			newNodes = newNodes.Where(x => !closedNodes.ContainsKey(GetCellId(x.m_Pos))).ToList();

			foreach (PathFindingNode n in newNodes)
			{
				unusedNodes.Add(GetCellId(n.m_Pos), n);
			}

			unusedNodes = unusedNodes.OrderBy(x => x.Value.m_FDist).ThenBy(z => z.Value.m_GDist).ThenBy(y => y.Value.m_HDist).ToDictionary(t => t.Key, t => t.Value);

			for (int i = 0; i < unusedNodes.Values.Count; i++)
			{
				if (!unusedNodes.Values.ElementAt(i).m_Closed)
				{
					path = unusedNodes.Values.ElementAt(i);
					if ((GetCellId(path.m_Pos) - GetCellId(end)).magnitude == 0)
					{
						found = true;
					}
					else
					{
						path.m_Closed = true;
						closedNodes.Add(GetCellId(path.m_Pos), path);
						unusedNodes.Remove(GetCellId(path.m_Pos));
					}
					break;
				}
			}

			count++;
		}

		List<PathFindingNode> points = new List<PathFindingNode>();

		while (path != null)
		{
			points.Insert(0, path);
			path = path.m_Parent;
		}

		return points;
	}

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
