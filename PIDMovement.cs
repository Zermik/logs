using System;
using Pathfinding.Drawing;
using Pathfinding.Util;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Profiling;
using UnityEngine;

namespace Pathfinding.PID
{
	// Token: 0x0200024A RID: 586
	[BurstCompile]
	[Serializable]
	public struct PIDMovement
	{
		// Token: 0x170001E1 RID: 481
		// (get) Token: 0x06000D8F RID: 3471 RVA: 0x000565C1 File Offset: 0x000547C1
		// (set) Token: 0x06000D90 RID: 3472 RVA: 0x000565CC File Offset: 0x000547CC
		public bool allowRotatingOnSpot
		{
			get
			{
				return this.allowRotatingOnSpotBacking > 0;
			}
			set
			{
				this.allowRotatingOnSpotBacking = (value ? 1 : 0);
			}
		}

		// Token: 0x06000D91 RID: 3473 RVA: 0x000565DC File Offset: 0x000547DC
		public void ScaleByAgentScale(float agentScale)
		{
			this.speed *= agentScale;
			this.leadInRadiusWhenApproachingDestination *= agentScale;
			this.desiredWallDistance *= agentScale;
		}

		// Token: 0x06000D92 RID: 3474 RVA: 0x00056608 File Offset: 0x00054808
		public float Speed(float remainingDistance)
		{
			if (this.speed <= 0f)
			{
				return 0f;
			}
			if (this.slowdownTime > 0f)
			{
				float num = Mathf.Min(1f, Mathf.Sqrt(2f * remainingDistance / (this.speed * this.slowdownTime)));
				return this.speed * num;
			}
			if (remainingDistance > 0.0001f)
			{
				return this.speed;
			}
			return 0f;
		}

		// Token: 0x06000D93 RID: 3475 RVA: 0x00056678 File Offset: 0x00054878
		public float Accelerate(float speed, float timeToReachMaxSpeed, float dt)
		{
			if (timeToReachMaxSpeed > 0.001f)
			{
				float num = this.speed / timeToReachMaxSpeed;
				return math.clamp(speed + dt * num, 0f, this.speed);
			}
			if (dt <= 0f)
			{
				return 0f;
			}
			return this.speed;
		}

		// Token: 0x06000D94 RID: 3476 RVA: 0x000566C0 File Offset: 0x000548C0
		public float CurveFollowingStrength(float signedDistToClearArea, float radiusToWall, float remainingDistance)
		{
			float num = math.max(1E-05f, this.speed);
			float x = math.max(AnglePIDController.RotationSpeedToFollowingStrength(num, math.radians(this.rotationSpeed)), 40f * math.pow(math.abs(signedDistToClearArea) / math.max(0.0001f, radiusToWall), 1f));
			float num2 = remainingDistance / num;
			return math.max(x, math.min(80f, math.pow(1f / math.max(0f, num2 - 0.2f), 3f)));
		}

		// Token: 0x06000D95 RID: 3477 RVA: 0x0005674C File Offset: 0x0005494C
		private static bool ClipLineByHalfPlaneX(ref float2 a, ref float2 b, float x, float side)
		{
			bool flag = (a.x - x) * side < 0f;
			bool flag2 = (b.x - x) * side < 0f;
			if (flag && flag2)
			{
				return false;
			}
			if (flag != flag2)
			{
				float t = math.unlerp(a.x, b.x, x);
				float2 @float = math.lerp(a, b, t);
				if (flag)
				{
					a = @float;
				}
				else
				{
					b = @float;
				}
			}
			return true;
		}

		// Token: 0x06000D96 RID: 3478 RVA: 0x000567C4 File Offset: 0x000549C4
		private static void ClipLineByHalfPlaneYt(float2 a, float2 b, float y, float side, ref float mnT, ref float mxT)
		{
			bool flag = (a.y - y) * side < 0f;
			bool flag2 = (b.y - y) * side < 0f;
			if (flag && flag2)
			{
				mnT = 1f;
				mxT = 0f;
				return;
			}
			if (flag != flag2)
			{
				float y2 = math.unlerp(a.y, b.y, y);
				if (flag)
				{
					mnT = math.max(mnT, y2);
					return;
				}
				mxT = math.min(mxT, y2);
			}
		}

		// Token: 0x06000D97 RID: 3479 RVA: 0x0005683E File Offset: 0x00054A3E
		private static float2 MaxAngle(float2 a, float2 b, float2 c, bool clockwise)
		{
			a = math.select(a, b, VectorMath.Determinant(a, b) < 0f == clockwise);
			a = math.select(a, c, VectorMath.Determinant(a, c) < 0f == clockwise);
			return a;
		}

		// Token: 0x06000D98 RID: 3480 RVA: 0x00056875 File Offset: 0x00054A75
		private static float2 MaxAngle(float2 a, float2 b, bool clockwise)
		{
			return math.select(a, b, VectorMath.Determinant(a, b) < 0f == clockwise);
		}

		// Token: 0x06000D99 RID: 3481 RVA: 0x00056890 File Offset: 0x00054A90
		private static void DrawChisel(float2 start, float2 direction, float pointiness, float length, float width, CommandBuilder draw, Color col)
		{
			draw.PushColor(col);
			float2 @float = start + (direction * pointiness + new float2(-direction.y, direction.x)) * width;
			float2 float2 = start + (direction * pointiness - new float2(-direction.y, direction.x)) * width;
			draw.xz.Line(start, @float, col);
			draw.xz.Line(start, float2, col);
			float num = length - pointiness * width;
			if (num > 0f)
			{
				draw.xz.Ray(@float, direction * num, col);
				draw.xz.Ray(float2, direction * num, col);
			}
			draw.PopColor();
		}

		// Token: 0x06000D9A RID: 3482 RVA: 0x0005696C File Offset: 0x00054B6C
		private static void SplitSegment(float2 e1, float2 e2, float desiredRadius, float length, float pointiness, ref PIDMovement.EdgeBuffers buffers)
		{
			float num = desiredRadius * 2f;
			if ((e1.y < -num && e2.y < -num) || (e1.y > num && e2.y > num))
			{
				return;
			}
			if (!PIDMovement.ClipLineByHalfPlaneX(ref e1, ref e2, 0f, 1f))
			{
				return;
			}
			float num2;
			float num3;
			if (!VectorMath.SegmentCircleIntersectionFactors(e1, e2, length * length, out num2, out num3))
			{
				return;
			}
			float num4 = desiredRadius * 0.01f;
			float num5;
			float num6;
			if (VectorMath.SegmentCircleIntersectionFactors(e1, e2, num4 * num4, out num5, out num6) && num5 < num3 && num6 > num2)
			{
				if (num5 > num2 && num5 < num3)
				{
					PIDMovement.SplitSegment2(math.lerp(e1, e2, num2), math.lerp(e1, e2, num5), desiredRadius, pointiness, ref buffers);
				}
				if (num6 > num2 && num6 < num3)
				{
					PIDMovement.SplitSegment2(math.lerp(e1, e2, num6), math.lerp(e1, e2, num3), desiredRadius, pointiness, ref buffers);
					return;
				}
			}
			else
			{
				PIDMovement.SplitSegment2(math.lerp(e1, e2, num2), math.lerp(e1, e2, num3), desiredRadius, pointiness, ref buffers);
			}
		}

		// Token: 0x06000D9B RID: 3483 RVA: 0x00056A58 File Offset: 0x00054C58
		private static void SplitSegment2(float2 e1, float2 e2, float desiredRadius, float pointiness, ref PIDMovement.EdgeBuffers buffers)
		{
			float num;
			float num2;
			if (!VectorMath.SegmentCircleIntersectionFactors(e1, e2, (pointiness * pointiness + 1f) * desiredRadius * desiredRadius, out num, out num2))
			{
				PIDMovement.SplitSegment3(e1, e2, desiredRadius, false, ref buffers);
				return;
			}
			if (num > 0f && num2 < 1f)
			{
				PIDMovement.SplitSegment3(e1, math.lerp(e1, e2, num), desiredRadius, false, ref buffers);
				PIDMovement.SplitSegment3(math.lerp(e1, e2, num), math.lerp(e1, e2, num2), desiredRadius, true, ref buffers);
				PIDMovement.SplitSegment3(math.lerp(e1, e2, num2), e2, desiredRadius, false, ref buffers);
				return;
			}
			if (num > 0f)
			{
				PIDMovement.SplitSegment3(e1, math.lerp(e1, e2, num), desiredRadius, false, ref buffers);
				PIDMovement.SplitSegment3(math.lerp(e1, e2, num), e2, desiredRadius, true, ref buffers);
				return;
			}
			if (num2 < 1f)
			{
				PIDMovement.SplitSegment3(e1, math.lerp(e1, e2, num2), desiredRadius, true, ref buffers);
				PIDMovement.SplitSegment3(math.lerp(e1, e2, num2), e2, desiredRadius, false, ref buffers);
				return;
			}
			PIDMovement.SplitSegment3(e1, e2, desiredRadius, true, ref buffers);
		}

		// Token: 0x06000D9C RID: 3484 RVA: 0x00056B44 File Offset: 0x00054D44
		private static void SplitSegment3(float2 e1, float2 e2, float desiredRadius, bool inTriangularRegion, ref PIDMovement.EdgeBuffers buffers)
		{
			float2 @float = e1;
			float2 float2 = e2;
			if (float2.x < @float.x)
			{
				@float.y -= 0.01f;
				float2.y -= 0.01f;
			}
			else
			{
				@float.y += 0.01f;
				float2.y += 0.01f;
			}
			bool flag = @float.y > 0f;
			if (!flag)
			{
				Memory.Swap<float2>(ref e1, ref e2);
				Memory.Swap<float2>(ref @float, ref float2);
			}
			float num = math.unlerp(@float.y, float2.y, 0f);
			bool flag2 = math.isfinite(num);
			if (num <= 0f || num >= 1f || !flag2)
			{
				PIDMovement.SplitSegment4(e1, e2, inTriangularRegion, flag, ref buffers);
				return;
			}
			float2 float3 = e1 + num * (e2 - e1);
			float num2 = math.lengthsq(e1 - float3);
			float num3 = math.lengthsq(e2 - float3);
			float num4 = desiredRadius * 0.1f;
			float num5 = num4 * num4;
			if (num2 > num5 || num2 >= num3)
			{
				PIDMovement.SplitSegment4(e1, float3, inTriangularRegion, true, ref buffers);
			}
			if (num3 > num5 || num3 >= num2)
			{
				PIDMovement.SplitSegment4(float3, e2, inTriangularRegion, false, ref buffers);
			}
		}

		// Token: 0x06000D9D RID: 3485 RVA: 0x00056C78 File Offset: 0x00054E78
		private static void SplitSegment4(float2 e1, float2 e2, bool inTriangularRegion, bool left, ref PIDMovement.EdgeBuffers buffers)
		{
			if (math.all(math.abs(e1 - e2) < 0.01f))
			{
				return;
			}
			ref FixedList512Bytes<float2> ptr = ref buffers.triangleRegionEdgesL;
			if (inTriangularRegion)
			{
				if (!left)
				{
					ptr = ref buffers.triangleRegionEdgesR;
				}
			}
			else if (left)
			{
				ptr = ref buffers.straightRegionEdgesL;
			}
			else
			{
				ptr = ref buffers.straightRegionEdgesR;
			}
			if (ptr.Length + 2 > ptr.Capacity)
			{
				return;
			}
			ptr.AddNoResize(e1);
			ptr.AddNoResize(e2);
		}

		// Token: 0x06000D9E RID: 3486 RVA: 0x00056CF4 File Offset: 0x00054EF4
		public static float2 OptimizeDirection(float2 start, float2 end, float desiredRadius, float remainingDistance, float pointiness, NativeArray<float2> edges, CommandBuilder draw, PIDMovement.DebugFlags debugFlags)
		{
			float num = math.length(end - start);
			float2 @float = math.normalizesafe(end - start, default(float2));
			num *= 0.999f;
			num = math.min(0.9f * remainingDistance, num);
			if (desiredRadius <= 0.0001f)
			{
				return @float;
			}
			float num2 = num;
			float num3 = 1f / num2;
			PIDMovement.EdgeBuffers edgeBuffers = default(PIDMovement.EdgeBuffers);
			for (int i = 0; i < edges.Length; i += 2)
			{
				float2 e = VectorMath.ComplexMultiplyConjugate(edges[i] - start, @float);
				float2 e2 = VectorMath.ComplexMultiplyConjugate(edges[i + 1] - start, @float);
				PIDMovement.SplitSegment(e, e2, desiredRadius, num, pointiness, ref edgeBuffers);
			}
			float2 float2 = new float2(1f, 0f);
			for (int j = 0; j < 8; j++)
			{
				if ((debugFlags & PIDMovement.DebugFlags.ForwardClearance) != PIDMovement.DebugFlags.Nothing)
				{
					Color blue = Palette.Colorbrewer.Set1.Blue;
					blue.a = 0.5f;
					float2 float3 = VectorMath.ComplexMultiply(float2, @float);
					PIDMovement.DrawChisel(start, float3, pointiness, num, desiredRadius, draw, blue);
					draw.xz.Ray(start, float3 * num, Palette.Colorbrewer.Set1.Purple);
					draw.xz.Circle(start, remainingDistance, blue);
				}
				float2 rhs = new float2(0f, desiredRadius);
				float2 rhs2 = new float2(0f, -desiredRadius);
				float2 float4 = new float2(num, 0f);
				float2 float5 = new float2(num, 0f);
				for (int k = 0; k < edgeBuffers.straightRegionEdgesL.Length; k += 2)
				{
					float2 lhs = VectorMath.ComplexMultiplyConjugate(edgeBuffers.straightRegionEdgesL[k], float2);
					float2 lhs2 = VectorMath.ComplexMultiplyConjugate(edgeBuffers.straightRegionEdgesL[k + 1], float2);
					float4 = PIDMovement.MaxAngle(float4, lhs - rhs, lhs2 - rhs, true);
				}
				for (int l = 0; l < edgeBuffers.straightRegionEdgesR.Length; l += 2)
				{
					float2 lhs3 = VectorMath.ComplexMultiplyConjugate(edgeBuffers.straightRegionEdgesR[l], float2);
					float2 lhs4 = VectorMath.ComplexMultiplyConjugate(edgeBuffers.straightRegionEdgesR[l + 1], float2);
					float5 = PIDMovement.MaxAngle(float5, lhs3 - rhs2, lhs4 - rhs2, false);
				}
				float2 b = math.normalizesafe(VectorMath.ComplexMultiply(new float2(pointiness * desiredRadius, desiredRadius), float2), default(float2));
				float2 b2 = math.normalizesafe(VectorMath.ComplexMultiply(new float2(pointiness * desiredRadius, -desiredRadius), float2), default(float2));
				for (int m = 0; m < edgeBuffers.triangleRegionEdgesL.Length; m += 2)
				{
					float2 float6 = VectorMath.ComplexMultiplyConjugate(edgeBuffers.triangleRegionEdgesL[m], b);
					float2 float7 = VectorMath.ComplexMultiplyConjugate(edgeBuffers.triangleRegionEdgesL[m + 1], b);
					float2 float8 = (float7.y < float6.y) ? float7 : float6;
					if (float8.y < 0f)
					{
						float4 = PIDMovement.MaxAngle(float4, float8, true);
					}
				}
				for (int n = 0; n < edgeBuffers.triangleRegionEdgesR.Length; n += 2)
				{
					float2 float9 = VectorMath.ComplexMultiplyConjugate(edgeBuffers.triangleRegionEdgesR[n], b2);
					float2 float10 = VectorMath.ComplexMultiplyConjugate(edgeBuffers.triangleRegionEdgesR[n + 1], b2);
					float2 float11 = (float10.y > float9.y) ? float10 : float9;
					if (float11.y > 0f)
					{
						float5 = PIDMovement.MaxAngle(float5, float11, false);
					}
				}
				float rhs3 = 1f / math.max(1E-06f, num2 - float4.x * float4.x) - num3;
				float rhs4 = 1f / math.max(1E-06f, num2 - float5.x * float5.x) - num3;
				float2 end2 = math.normalizesafe(float4 * rhs4 + float5 * rhs3, default(float2));
				float2 b3 = math.lerp(new float2(1f, 0f), end2, 1f);
				float2 = math.normalizesafe(VectorMath.ComplexMultiply(float2, b3), default(float2));
				if (float4.y == 0f && float5.y == 0f)
				{
					num = math.min(remainingDistance * 0.9f, math.min(num * 1.1f, num2 * 1.2f));
				}
				else
				{
					num = math.min(num, math.max(desiredRadius * 2f, math.min(float4.x, float5.x) * 2f));
				}
			}
			float2 = VectorMath.ComplexMultiply(float2, @float);
			if ((debugFlags & PIDMovement.DebugFlags.ForwardClearance) != PIDMovement.DebugFlags.Nothing)
			{
				PIDMovement.DrawChisel(start, float2, pointiness, num, desiredRadius, draw, Color.black);
			}
			return float2;
		}

		// Token: 0x06000D9F RID: 3487 RVA: 0x000571B0 File Offset: 0x000553B0
		public static float SmallestDistanceWithinWedge(float2 point, float2 dir1, float2 dir2, float shrinkAmount, NativeArray<float2> edges)
		{
			dir1 = math.normalizesafe(dir1, default(float2));
			dir2 = math.normalizesafe(dir2, default(float2));
			if (math.dot(dir1, dir2) > 0.999f)
			{
				return float.PositiveInfinity;
			}
			float num = math.sign(VectorMath.Determinant(dir1, dir2));
			shrinkAmount *= num;
			float num2 = float.PositiveInfinity;
			for (int i = 0; i < edges.Length; i += 2)
			{
				float2 @float = edges[i] - point;
				float2 float2 = edges[i + 1] - point;
				float2 a = VectorMath.ComplexMultiplyConjugate(@float, dir1);
				float2 b = VectorMath.ComplexMultiplyConjugate(float2, dir1);
				float2 a2 = VectorMath.ComplexMultiplyConjugate(@float, dir2);
				float2 b2 = VectorMath.ComplexMultiplyConjugate(float2, dir2);
				float num3 = 0f;
				float num4 = 1f;
				PIDMovement.ClipLineByHalfPlaneYt(a, b, shrinkAmount, num, ref num3, ref num4);
				if (num3 <= num4)
				{
					PIDMovement.ClipLineByHalfPlaneYt(a2, b2, -shrinkAmount, -num, ref num3, ref num4);
					if (num3 <= num4)
					{
						float num5 = math.lengthsq(float2 - @float);
						float t = math.clamp(math.dot(@float, @float - float2) * math.rcp(num5), num3, num4);
						float y = math.lengthsq(math.lerp(@float, float2, t));
						num2 = math.select(num2, math.min(num2, y), num5 > 1.1754944E-38f);
					}
				}
			}
			return math.sqrt(num2);
		}

		// Token: 0x06000DA0 RID: 3488 RVA: 0x00057308 File Offset: 0x00055508
		public static float2 Linecast(float2 a, float2 b, NativeArray<float2> edges)
		{
			float num = 1f;
			for (int i = 0; i < edges.Length; i += 2)
			{
				float2 @float = edges[i];
				float2 lhs = edges[i + 1];
				float num2;
				float num3;
				VectorMath.LineLineIntersectionFactors(a, b - a, @float, lhs - @float, out num2, out num3);
				if (num3 >= 0f && num3 <= 1f && num2 > 0f)
				{
					num = math.min(num, num2);
				}
			}
			return a + (b - a) * num;
		}

		// Token: 0x06000DA1 RID: 3489 RVA: 0x00057394 File Offset: 0x00055594
		public static Bounds InterestingEdgeBounds(ref PIDMovement settings, float3 position, float3 nextCorner, float height, NativeMovementPlane plane)
		{
			float3 @float = math.mul(math.conjugate(plane.rotation), position);
			float3 v = math.mul(math.conjugate(plane.rotation), nextCorner);
			Bounds result = new Bounds(@float + new float3(0f, height * 0.25f, 0f), new Vector3(0f, 1.5f * height, 0f));
			v.y = @float.y;
			result.Encapsulate(v);
			if (settings.rotationSpeed > 0f)
			{
				float x = settings.speed / math.radians(settings.rotationSpeed);
				result.Expand(new Vector3(1f, 0f, 1f) * math.max(x, settings.desiredWallDistance * 8f * 1f));
			}
			return result;
		}

		// Token: 0x06000DA2 RID: 3490 RVA: 0x00057478 File Offset: 0x00055678
		private static float2 OffsetCornerForApproach(float2 position2D, float2 endOfPath2D, float2 facingDir2D, ref PIDMovement settings, float2 nextCorner2D, ref float gammaAngle, ref float gammaAngleWeight, PIDMovement.DebugFlags debugFlags, ref CommandBuilder draw, NativeArray<float2> edges)
		{
			float2 @float = endOfPath2D - position2D;
			if (math.dot(math.normalizesafe(@float, default(float2)), facingDir2D) < -0.2f)
			{
				return nextCorner2D;
			}
			float2 rhs = new float2(-@float.y, @float.x);
			float2 float2 = new float2(-facingDir2D.y, facingDir2D.x);
			float2 float3 = (position2D + endOfPath2D) * 0.5f;
			bool flag;
			float2 float4 = VectorMath.LineIntersectionPoint(float3, float3 + rhs, endOfPath2D, endOfPath2D + float2, out flag);
			if (!flag)
			{
				return nextCorner2D;
			}
			float num = PIDMovement.SmallestDistanceWithinWedge(endOfPath2D - 0.01f * facingDir2D, float2 - 0.1f * facingDir2D, -float2 - 0.1f * facingDir2D, 0.001f, edges);
			float x = settings.leadInRadiusWhenApproachingDestination;
			x = math.min(x, num * 0.9f);
			float num2 = math.length(float4 - endOfPath2D);
			float num3 = math.abs(math.dot(math.normalizesafe(@float, default(float2)), float2));
			float num4 = 1f / math.sqrt(1f - num3 * num3) * math.length(@float) * 0.5f;
			num4 /= math.min(x, num2);
			num4 = math.tanh(num4);
			num4 *= math.min(x, num2);
			float2 float5 = nextCorner2D - facingDir2D * num4;
			if ((debugFlags & PIDMovement.DebugFlags.ApproachWithOrientation) != PIDMovement.DebugFlags.Nothing)
			{
				draw.xz.Circle(float4, num2, Color.blue);
				draw.xz.Arrow(position2D, float5, Palette.Colorbrewer.Set1.Orange);
			}
			if (math.lengthsq(PIDMovement.Linecast(position2D, float5, edges) - float5) > 0.01f)
			{
				return nextCorner2D;
			}
			return float5;
		}

		// Token: 0x06000DA3 RID: 3491 RVA: 0x00057664 File Offset: 0x00055864
		public static AnglePIDControlOutput2D Control(ref PIDMovement settings, float dt, ref PIDMovement.ControlParams controlParams, ref CommandBuilder draw, out float maxDesiredWallDistance)
		{
			if (dt <= 0f)
			{
				maxDesiredWallDistance = controlParams.maxDesiredWallDistance;
				AnglePIDControlOutput2D anglePIDControlOutput2D = new AnglePIDControlOutput2D
				{
					rotationDelta = 0f,
					positionDelta = float2.zero
				};
				return anglePIDControlOutput2D;
			}
			NativeMovementPlane movementPlane = controlParams.movementPlane;
			float y;
			float2 @float = movementPlane.ToPlane(controlParams.p, out y);
			if (controlParams.debugFlags != PIDMovement.DebugFlags.Nothing)
			{
				draw.PushMatrix(math.mul(new float4x4(movementPlane.rotation, float3.zero), float4x4.Translate(new float3(0f, y, 0f))));
			}
			if ((controlParams.debugFlags & PIDMovement.DebugFlags.Position) != PIDMovement.DebugFlags.Nothing)
			{
				draw.xz.Cross(controlParams.closestOnNavmesh, 0.05f, Color.red);
			}
			NativeArray<float2> edges = controlParams.edges;
			if ((controlParams.debugFlags & PIDMovement.DebugFlags.Obstacles) != PIDMovement.DebugFlags.Nothing)
			{
				draw.PushLineWidth(2f, true);
				draw.PushColor(Color.red);
				for (int i = 0; i < edges.Length; i += 2)
				{
					draw.xz.Line(edges[i], edges[i + 1]);
				}
				draw.PopColor();
				draw.PopLineWidth();
			}
			float2 float2 = movementPlane.ToPlane(controlParams.nextCorner);
			float curveCurvature = 0f;
			float angle = 0f;
			float num = 0f;
			float num2 = controlParams.rotation + 1.5707964f;
			float2 float3 = math.normalizesafe(movementPlane.ToPlane(controlParams.facingDirectionAtEndOfPath), default(float2));
			bool flag = controlParams.remainingDistance < controlParams.agentRadius * 0.1f;
			if (!flag && settings.leadInRadiusWhenApproachingDestination > 0f && math.any(float3 != 0f))
			{
				float2 float4 = movementPlane.ToPlane(controlParams.endOfPath);
				if (math.lengthsq(float4 - float2) <= 0.1f)
				{
					float2 float5 = PIDMovement.OffsetCornerForApproach(@float, float4, float3, ref settings, float2, ref angle, ref num, controlParams.debugFlags, ref draw, edges);
					float2 = float5;
					float num3 = settings.speed * 0.1f;
					if (num3 > 0.001f)
					{
						float y2;
						float x;
						math.sincos(num2, out y2, out x);
						float2 lhs = new float2(x, y2);
						float2 lhs2 = PIDMovement.OffsetCornerForApproach(@float + lhs * num3, float4, float3, ref settings, float2, ref angle, ref num, PIDMovement.DebugFlags.Nothing, ref draw, edges);
						curveCurvature = math.asin(VectorMath.Determinant(math.normalizesafe(float5 - @float, default(float2)), math.normalizesafe(lhs2 - @float, default(float2)))) / num3;
					}
				}
			}
			float num4 = settings.desiredWallDistance;
			num4 = math.max(0f, math.min(num4, (controlParams.remainingDistance - num4) / 4f));
			float2 = PIDMovement.Linecast(@float, float2, edges);
			float2 float6 = PIDMovement.OptimizeDirection(@float, float2, num4, controlParams.remainingDistance, 2f, edges, draw, controlParams.debugFlags);
			maxDesiredWallDistance = controlParams.maxDesiredWallDistance + settings.speed * 0.1f * dt;
			float num5 = maxDesiredWallDistance;
			float curveDistanceSigned = 0f;
			float signedDistToClearArea = 0f;
			maxDesiredWallDistance = math.min(maxDesiredWallDistance, num5);
			if ((controlParams.debugFlags & PIDMovement.DebugFlags.Tangent) != PIDMovement.DebugFlags.Nothing)
			{
				draw.Arrow(controlParams.p, controlParams.p + new Vector3(float6.x, 0f, float6.y), Palette.Colorbrewer.Set1.Orange);
			}
			AnglePIDControlOutput2D result;
			if (flag)
			{
				float num6 = math.min(settings.Speed(controlParams.remainingDistance), settings.Accelerate(controlParams.speed, settings.slowdownTime, dt));
				float2 float7 = float2 - @float;
				float num7 = math.length(float7);
				if (math.any(float3 != 0f))
				{
					float num8 = math.atan2(float3.y, float3.x);
					float num9 = dt * math.radians(settings.maxRotationSpeed);
					AnglePIDControlOutput2D anglePIDControlOutput2D = new AnglePIDControlOutput2D
					{
						rotationDelta = math.clamp(AstarMath.DeltaAngle(num2, num8), -num9, num9),
						targetRotation = num8 - 1.5707964f,
						positionDelta = ((num7 > 1.1754944E-38f) ? (float7 * (dt * num6 / num7)) : float7)
					};
					result = anglePIDControlOutput2D;
				}
				else
				{
					AnglePIDControlOutput2D anglePIDControlOutput2D = new AnglePIDControlOutput2D
					{
						rotationDelta = 0f,
						targetRotation = num2 - 1.5707964f,
						positionDelta = ((num7 > 1.1754944E-38f) ? (float7 * (dt * num6 / num7)) : float7)
					};
					result = anglePIDControlOutput2D;
				}
			}
			else
			{
				float followingStrength = settings.CurveFollowingStrength(signedDistToClearArea, num5, controlParams.remainingDistance);
				float num10 = math.atan2(float6.y, float6.x);
				float minRotationSpeed = 0f;
				if (math.abs(AstarMath.DeltaAngle(num10, num2)) > 0.003141593f)
				{
					float y3;
					float x2;
					math.sincos(num2, out y3, out x2);
					float2 float8 = new float2(x2, y3);
					float num11 = PIDMovement.SmallestDistanceWithinWedge(@float, float6, float8, controlParams.agentRadius * 0.1f, edges);
					if ((controlParams.debugFlags & PIDMovement.DebugFlags.ForwardClearance) != PIDMovement.DebugFlags.Nothing && float.IsFinite(num11))
					{
						draw.xz.Arc(@float, @float + float8 * num11, @float + float6, Palette.Colorbrewer.Set1.Purple);
					}
					if (num11 > 0.001f && num11 * 1.01f < controlParams.remainingDistance)
					{
						minRotationSpeed = math.rcp(num11) * 2f;
					}
				}
				result = AnglePIDController.Control(ref settings, followingStrength, num2, num10 + AstarMath.DeltaAngle(num10, angle) * num, curveCurvature, curveDistanceSigned, controlParams.speed, controlParams.remainingDistance, minRotationSpeed, controlParams.speed < settings.speed * 0.1f, dt);
				result.targetRotation -= 1.5707964f;
			}
			if (controlParams.debugFlags != PIDMovement.DebugFlags.Nothing)
			{
				draw.PopMatrix();
			}
			return result;
		}

		// Token: 0x04000A73 RID: 2675
		public float rotationSpeed;

		// Token: 0x04000A74 RID: 2676
		public float speed;

		// Token: 0x04000A75 RID: 2677
		public float maxRotationSpeed;

		// Token: 0x04000A76 RID: 2678
		public float maxOnSpotRotationSpeed;

		// Token: 0x04000A77 RID: 2679
		public float slowdownTime;

		// Token: 0x04000A78 RID: 2680
		public float slowdownTimeWhenTurningOnSpot;

		// Token: 0x04000A79 RID: 2681
		public float desiredWallDistance;

		// Token: 0x04000A7A RID: 2682
		public float leadInRadiusWhenApproachingDestination;

		// Token: 0x04000A7B RID: 2683
		[SerializeField]
		private byte allowRotatingOnSpotBacking;

		// Token: 0x04000A7C RID: 2684
		public const float DESTINATION_CLEARANCE_FACTOR = 4f;

		// Token: 0x04000A7D RID: 2685
		private static readonly ProfilerMarker MarkerSidewaysAvoidance = new ProfilerMarker("SidewaysAvoidance");

		// Token: 0x04000A7E RID: 2686
		private static readonly ProfilerMarker MarkerPID = new ProfilerMarker("PID");

		// Token: 0x04000A7F RID: 2687
		private static readonly ProfilerMarker MarkerOptimizeDirection = new ProfilerMarker("OptimizeDirection");

		// Token: 0x04000A80 RID: 2688
		private static readonly ProfilerMarker MarkerSmallestDistance = new ProfilerMarker("ClosestDistance");

		// Token: 0x04000A81 RID: 2689
		private static readonly ProfilerMarker MarkerConvertObstacles = new ProfilerMarker("ConvertObstacles");

		// Token: 0x04000A82 RID: 2690
		private const float ALLOWED_OVERLAP_FACTOR = 0.1f;

		// Token: 0x04000A83 RID: 2691
		private const float STEP_MULTIPLIER = 1f;

		// Token: 0x04000A84 RID: 2692
		private const float MAX_FRACTION_OF_REMAINING_DISTANCE = 0.9f;

		// Token: 0x04000A85 RID: 2693
		private const int OPTIMIZATION_ITERATIONS = 8;

		// Token: 0x0200024B RID: 587
		public struct PersistentState
		{
			// Token: 0x04000A86 RID: 2694
			public float maxDesiredWallDistance;
		}

		// Token: 0x0200024C RID: 588
		[Flags]
		public enum DebugFlags
		{
			// Token: 0x04000A88 RID: 2696
			Nothing = 0,
			// Token: 0x04000A89 RID: 2697
			Position = 1,
			// Token: 0x04000A8A RID: 2698
			Tangent = 2,
			// Token: 0x04000A8B RID: 2699
			SidewaysClearance = 4,
			// Token: 0x04000A8C RID: 2700
			ForwardClearance = 8,
			// Token: 0x04000A8D RID: 2701
			Obstacles = 16,
			// Token: 0x04000A8E RID: 2702
			Funnel = 32,
			// Token: 0x04000A8F RID: 2703
			Path = 64,
			// Token: 0x04000A90 RID: 2704
			ApproachWithOrientation = 128,
			// Token: 0x04000A91 RID: 2705
			Rotation = 256
		}

		// Token: 0x0200024D RID: 589
		private struct EdgeBuffers
		{
			// Token: 0x04000A92 RID: 2706
			public FixedList512Bytes<float2> triangleRegionEdgesL;

			// Token: 0x04000A93 RID: 2707
			public FixedList512Bytes<float2> triangleRegionEdgesR;

			// Token: 0x04000A94 RID: 2708
			public FixedList512Bytes<float2> straightRegionEdgesL;

			// Token: 0x04000A95 RID: 2709
			public FixedList512Bytes<float2> straightRegionEdgesR;
		}

		// Token: 0x0200024E RID: 590
		public struct ControlParams
		{
			// Token: 0x04000A96 RID: 2710
			public Vector3 p;

			// Token: 0x04000A97 RID: 2711
			public float speed;

			// Token: 0x04000A98 RID: 2712
			public float rotation;

			// Token: 0x04000A99 RID: 2713
			public float maxDesiredWallDistance;

			// Token: 0x04000A9A RID: 2714
			public float3 endOfPath;

			// Token: 0x04000A9B RID: 2715
			public float3 facingDirectionAtEndOfPath;

			// Token: 0x04000A9C RID: 2716
			public NativeArray<float2> edges;

			// Token: 0x04000A9D RID: 2717
			public float3 nextCorner;

			// Token: 0x04000A9E RID: 2718
			public float agentRadius;

			// Token: 0x04000A9F RID: 2719
			public float remainingDistance;

			// Token: 0x04000AA0 RID: 2720
			public float3 closestOnNavmesh;

			// Token: 0x04000AA1 RID: 2721
			public PIDMovement.DebugFlags debugFlags;

			// Token: 0x04000AA2 RID: 2722
			public NativeMovementPlane movementPlane;
		}
	}
}
