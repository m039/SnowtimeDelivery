using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Runtime.Versioning;

namespace Game1
{
	public class Snowman : IHasRectWs
	{
		public bool _isFacingRight = true;
		public float timeSpentInAir = 0f;
		public Vector2 pos = new Vector2(0f, 0f);
		public Vector2 vel = new Vector2(0f, 0f);
		public float walkAnimEvalTime = 0f;
		public bool wasGrounded = false;
		public bool isDead = false;
		public bool wasDead = false;
		public float timeSpentDead = 0f;
		private int jumpCounter = 0;
		public bool isCrouched = false;

		public Rectf GetRectWs() {
			Rectf res = new Rectf();
			if (isCrouched) {
				res.X = pos.X + 8f;
				res.Y = pos.Y + 16f;
				res.Width = 16f;
				res.Height = 16f;
				return res;
			}
			else {
				res.X = pos.X + 8f;
				res.Y = pos.Y + 4f;
				res.Width = 16f;
				res.Height = 28f;
				return res;
			}
		}

		public void CollisionTestEnemies<T> (List<T> enemies, Level level) where T : IHasRectWs {
			Rectf snowmanRect = GetRectWs();
			foreach (var f in enemies) {
				Rectf r = f.GetRectWs();
				Vector2 depth = snowmanRect.GetIntersectionDepth(r);
				if (depth != Vector2.Zero && level.isComplete == false) {
					isDead = true;
				}
			}
		}

		public void CollisionTiles<T>(List<T> tiles, ref bool isGrounded) where T : IHasRectWs {

			var argsGetRect = new object[] { };
			foreach (T tile in tiles) {

				bool isOneWay = tile is OneWayTile;

				Rectf snowmanRect = GetRectWs();
				var isSolidInfo = tile.GetType().GetField("_isSolid");
				bool isSolid = true;

				if (isSolidInfo != null) {
					isSolid = (bool)isSolidInfo.GetValue(tile);
				}

				if (isSolid == true) {
					Rectf r = tile.GetRectWs();

					Vector2 depth = snowmanRect.GetIntersectionDepth(r);
					if (isOneWay) {
						if (depth.Y < 0f && MathF.Abs(depth.Y) < MathF.Abs(depth.X)) {
							if(depth.Y > -4f)
								pos.Y += depth.Y;
							isGrounded = depth.Y < 0f;
							if (vel.Y > 0f && depth.Y < 0f)
								vel.Y = 0;

							if (vel.Y < 0f && depth.Y > 0f && depth.X == 0)
								vel.Y = 0;
						}
					}
					else {
						if (depth.X != 0f && MathF.Abs(depth.X) < MathF.Abs(depth.Y)) {
							pos.X += depth.X;
							//vel.X = 0;
						}
						if (depth.Y != 0f && MathF.Abs(depth.Y) < MathF.Abs(depth.X)) {
							pos.Y += depth.Y;
							isGrounded = depth.Y < 0f;
							if (vel.Y > 0f && depth.Y < 0f)
								vel.Y = 0;

							if (vel.Y < 0f && depth.Y > 0f && depth.X == 0)
								vel.Y = 0;
						}
					}
				}
			}
		}

		public void Update(GameUpdateSets u) {

			if (u.level.timeSpentPlaying <= 0.75f) {
				u.level.camera.viewHeigthWs = MyMath.lerp(u.level.timeSpentPlaying / 0.75f, 160f, 240f);
			}
			else {
				u.level.camera.viewHeigthWs = 240;
			}

			float jumpHeight = 36f;
			float jumpTimeApex = 0.3f;
			float minJumpHeight = 8f;
			float fallingGravityMultiplier = 1f;
			float gravity = 2f * jumpHeight / (jumpTimeApex * jumpTimeApex);
			float fallingGravity = gravity * fallingGravityMultiplier;
			float maxJumpVelocity = gravity * jumpTimeApex;
			float minJumpVelocity = System.MathF.Sqrt(2f * gravity * minJumpHeight);

			// Update
			if (isDead) {
				vel = Vector2.Zero;
				timeSpentDead += u.dt;

				if (timeSpentDead > 1f) {
					u.level.shouldRestart = true;
				}
				return;
			}

			// Input, don't take it if the level is complete.

#if false
			if (InputSystem.IsResetButtonDown()) {
				u.level.isComplete = true;
				u.level.timeSpentComplete = 100f;
			}
#endif

			if (u.level.isComplete == false) {
				var thumbsticks = InputSystem.GetCurrentGamePadThumbSticks();
				isCrouched = InputSystem.IsDownButtonDown() || (thumbsticks != null && thumbsticks.Value.Left.Y < -0.1f);

				if (!isCrouched && InputSystem.IsLeftButtonDown()) {
					vel.X = MyMath.lerpClamp(vel.X, -140f, 350 * u.dt);
					walkAnimEvalTime += (float)(u.dt);
					_isFacingRight = false;
				}
				if (!isCrouched && InputSystem.IsRightButtonDown()) {
					vel.X = MyMath.lerpClamp(vel.X, 140f, 350 * u.dt);
					walkAnimEvalTime += (float)(u.dt);
					_isFacingRight = true;
				}

				if (!isCrouched && thumbsticks != null && MathF.Abs(thumbsticks.Value.Left.X) > 0.01f) {
					float k = thumbsticks.Value.Left.X;
					vel.X = MyMath.lerpClamp(vel.X, 140f * k, 350 * u.dt);
					walkAnimEvalTime += (float)(u.dt);
					_isFacingRight = k > 0f;
				}

				bool isJumpBtnPressed = InputSystem.IsJumpButtonDown();
				bool isJumpBtnReleased = InputSystem.IsJumpButtonUp();

				if ((timeSpentInAir < 0.15f || jumpCounter == 1) && isJumpBtnPressed) {
					// pressed
					vel.Y = -maxJumpVelocity;
					jumpCounter++;
					foreach (JumpSwitch tile in u.level.jumpSwitches) {
						tile._isSolid = !tile._isSolid;
					}
					u.game.jumpSfx.Play();
				}

				if (isJumpBtnReleased) {
					// released
					if (vel.Y < -minJumpVelocity) {
						vel.Y = -minJumpVelocity;
					}
				}
			}

			// Physics movement with no collisions (they are handled below).
			pos += vel * u.dt;

			if (vel.Y < 0)
				vel.Y += gravity * u.dt;
			else
				vel.Y += fallingGravity * u.dt;

			vel.X -= vel.X * (isCrouched ? 0.1f : 0.05f);

			// Collision check and response.
			bool isGrounded = false;

			// Non-harmful walkable tiles.
			CollisionTiles(u.level.tiles, ref isGrounded);
			CollisionTiles(u.level.oneWayTiles, ref isGrounded);
			CollisionTiles(u.level.jumpSwitches, ref isGrounded);
			CollisionTiles(u.level.timeSwitches, ref isGrounded);

			if (isGrounded) {
				jumpCounter = 0;
				timeSpentInAir = 0f;
			}
			else {
				timeSpentInAir += u.dt;
			}

			// Enemies collision and death.
			CollisionTestEnemies(u.level.walkers, u.level);
			CollisionTestEnemies(u.level.fires, u.level);
			CollisionTestEnemies(u.level.fireProjectiles, u.level);
			CollisionTestEnemies(u.level.ghosties, u.level);
			CollisionTestEnemies(u.level.iceSpikes, u.level);

			// Letters collecting
			foreach (Letter letter in u.level.letters) {
				if (letter.isCollected == false) {
					Rectf snowmanRect = GetRectWs();
					Vector2 depth = snowmanRect.GetIntersectionDepth(letter.GetRectWs());
					if (depth != Vector2.Zero) {
						letter.isCollected = true;
						u.game.pickupSfx.Play();
					}
				}
			}

			// Mailbox level ending.
			if(u.level.letterBox != null)
			{
				Rectf snowmanRect = GetRectWs();
				Vector2 depth = snowmanRect.GetIntersectionDepth(u.level.letterBox.GetRectWs());
				if (depth != Vector2.Zero) {

					bool areAllLeteterCollected = true;
					foreach (Letter l in u.level.letters) {
						areAllLeteterCollected &= l.isCollected;
					}
					if (areAllLeteterCollected && u.level.isComplete == false) {
						u.level.isComplete = true;
						u.game.levelWinSfx.Play();

						if (u.game.metrika != null)
						{
							u.game.metrika.reachGoal("levelComplete" + (u.game.currentLevel + 1));
						}

						if (u.game.bridge != null && u.game.bridge.leaderboard.isSupported)
						{
							if (u.game.bridge.platform.id == "yandex")
							{
								var options = new Dictionary<string, object>
                                {
                                    { "leaderboardName", "CompletedLevels" },
                                    { "score", u.game.currentLevel + 1}
                                };

								u.game.bridge.leaderboard.setScore(options);
							}
						}
					}
				}
			}

			// Kill the player if he falls. Don't kill him if the level is completed.
			if (pos.Y >= u.level.deathYCoord && !u.level.isComplete) {
				isDead = true;
			}

			// Acumulated values to check for events.
			if (isDead && !wasDead) {
				u.game.hitSfx.Play();
			}

			wasDead = isDead;
			wasGrounded = isGrounded;

			// Move the camera.
			Camera cam = u.level.camera;

			if (cam.pos.X - pos.X < -16f) {
				cam.pos.X = pos.X - 16f;
			}

			if (cam.pos.X - pos.X > 100f) {
				cam.pos.X = pos.X + 100f;
			}

			if (cam.pos.Y - (pos.Y + 50f) < -50f) {
				cam.pos.Y = (pos.Y + 50f) - 50f;
			}

			if (cam.pos.Y - pos.Y > 100f) {
				cam.pos.Y = pos.Y + 100f;
			}
		}
	}

}
