using Server;
using Server.Commands;
using Server.Commands.Generic;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using Server.Targeting;

using System;
using System.Collections;
using System.Collections.Generic;

namespace Server.Engine.Facet
{
	public class CircularIndentCommand : BaseCommand
	{
		public CircularIndentCommand()
		{
			AccessLevel = AccessLevel.GameMaster;
			Supports = CommandSupport.Simple;
			Commands = new string[] { "circularindent" };
			ObjectTypes = ObjectTypes.All;
			Usage = "circularindent <radius> <depth>";
			Description = "Makes a circular indent in the terrain. Click on the center point.";
		}

		public override bool ValidateArgs(BaseCommandImplementor impl, CommandEventArgs e)
		{
			if (e.Length != 2)
			{
				e.Mobile.SendMessage("Usage: circularindent <radius> <depth>");
				e.Mobile.SendMessage("Example: circularindent 10 -3");
				return false;
			}

			int radius = e.GetInt32(0);
			if (radius <= 0)
			{
				e.Mobile.SendMessage("Radius must be greater than 0.");
				return false;
			}

			return true;
		}

		public override void Execute(CommandEventArgs e, object obj)
		{
			int radius = e.GetInt32(0);
			int depth = e.GetInt32(1);

			Console.WriteLine($"[CircularIndent] Execute called: radius={radius}, depth={depth}, obj type={obj?.GetType().Name}");

			if (obj is IPoint3D location)
			{
				Console.WriteLine($"[CircularIndent] IPoint3D detected at X={location.X}, Y={location.Y}, Z={location.Z}");
				ApplyCircularIndent(e.Mobile, location.X, location.Y, location.Z, e.Mobile.Map, radius, depth);
			}
			else
			{
				Console.WriteLine($"[CircularIndent] Object is not IPoint3D, creating targeting cursor");
				e.Mobile.SendMessage("Target the center of where you want the circular indent.");
				e.Mobile.Target = new CircularIndentTarget(radius, depth);
			}
		}

		private static void ApplyCircularIndent(Mobile from, int centerX, int centerY, int centerZ, Map map, int radius, int depth)
		{
			try
			{
				Console.WriteLine($"[ApplyCircularIndent] Starting: map={map?.Name}, x={centerX}, y={centerY}, z={centerZ}, radius={radius}, depth={depth}");

				if (map == null)
				{
					Console.WriteLine($"[ApplyCircularIndent] ERROR: Map is null!");
					from.SendMessage("Error: Map is null.");
					return;
				}

				List<Point2D> circle = FacetEditingUtility.RasterFilledCircle(new Point2D(centerX, centerY), radius);

				Console.WriteLine($"[ApplyCircularIndent] Circle generated: {circle?.Count ?? 0} points");

				if (circle == null || circle.Count == 0)
				{
					Console.WriteLine($"[ApplyCircularIndent] ERROR: Circle is null or empty!");
					from.SendMessage("No terrain points generated for the specified radius.");
					return;
				}

				Console.WriteLine($"[ApplyCircularIndent] Creating MapOperationSeries with {circle.Count} points...");

				// Create the operation series with the first point
				MapOperationSeries operationSeries = new MapOperationSeries(
					new IncLandAltitude(circle[0].X, circle[0].Y, map.MapID, depth),
					map.MapID
				);

				// Add all remaining points
				for (int i = 1; i < circle.Count; i++)
				{
					operationSeries.Add(new IncLandAltitude(circle[i].X, circle[i].Y, map.MapID, depth));
				}

				Console.WriteLine($"[ApplyCircularIndent] Executing operation series...");

				// Execute all operations
				operationSeries.DoOperation();

				Console.WriteLine($"[ApplyCircularIndent] SUCCESS!");

				from.SendMessage(String.Format(
					"[SUCCESS] Circular indent applied: Center ({0}, {1}), Radius {2}, Depth {3}",
					centerX, centerY, radius, depth));
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[ApplyCircularIndent] EXCEPTION: {ex.Message}");
				Console.WriteLine($"[ApplyCircularIndent] StackTrace: {ex.StackTrace}");
				from.SendMessage($"Error: {ex.Message}");
			}
		}

		private class CircularIndentTarget : Target
		{
			private int m_Radius;
			private int m_Depth;

			public CircularIndentTarget(int radius, int depth) : base(50, false, TargetFlags.None)
			{
				m_Radius = radius;
				m_Depth = depth;
				Console.WriteLine($"[CircularIndentTarget] Created: radius={radius}, depth={depth}");
			}

			protected override void OnTarget(Mobile from, object targeted)
			{
				Console.WriteLine($"[CircularIndentTarget.OnTarget] Called! Target type: {targeted?.GetType().Name ?? "NULL"}");

				if (targeted is IPoint3D land)
				{
					Console.WriteLine($"[CircularIndentTarget.OnTarget] IPoint3D detected");
					Point3D p = (Point3D)land;
					ApplyCircularIndent(from, p.X, p.Y, p.Z, from.Map, m_Radius, m_Depth);
				}
				else
				{
					Console.WriteLine($"[CircularIndentTarget.OnTarget] Invalid target type: {targeted?.GetType().Name}");
					from.SendMessage("Invalid target. Please target terrain.");
				}
			}
		}
	}
}