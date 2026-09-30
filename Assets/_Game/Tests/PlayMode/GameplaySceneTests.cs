using System.Collections;
using NUnit.Framework;
using ParcelEscape.Gameplay;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ParcelEscape.Tests.PlayMode
{
    public class GameplaySceneTests
    {
        [UnityTest]
        public IEnumerator DevelopmentScene_RendersAndResolvesExpectedMoves()
        {
            yield return SceneManager.LoadSceneAsync("Gameplay");
            yield return null;

            GameObject boardSurface = GameObject.Find("BoardSurface");
            Assert.That(boardSurface, Is.Not.Null);
            Assert.That(boardSurface.transform.childCount, Is.EqualTo(25));
            Assert.That(GameObject.Find("Blocker_5"), Is.Not.Null);

            PackageView blue = FindPackage(1);
            PackageView red = FindPackage(2);
            PackageView green = FindPackage(3);
            PackageView yellow = FindPackage(4);

            Assert.That(blue.transform.position, Is.EqualTo(new Vector3(0f, 0.5f, 1f)));
            Assert.That(red.transform.position, Is.EqualTo(new Vector3(-2f, 0.5f, 0f)));
            Assert.That(green.transform.position, Is.EqualTo(new Vector3(0f, 0.5f, 0f)));
            Assert.That(yellow.transform.position, Is.EqualTo(new Vector3(-1f, 0.5f, 1f)));
            Assert.That(blue.transform.Find("DirectionArrow/Shaft"), Is.Not.Null);

            blue.OnTapped?.Invoke(blue.PackageId);
            blue.OnTapped?.Invoke(blue.PackageId);
            yield return new WaitForSeconds(0.7f);

            Assert.That(GameObject.Find("Package_1"), Is.Null,
                "The valid package should escape once, including under a duplicate request.");

            red.OnTapped?.Invoke(red.PackageId);
            green.OnTapped?.Invoke(green.PackageId);
            yellow.OnTapped?.Invoke(yellow.PackageId);
            yellow.OnTapped?.Invoke(yellow.PackageId);
            yield return new WaitForSeconds(0.5f);

            Assert.That(GameObject.Find("Package_2"), Is.Not.Null,
                "The package blocked by another package must remain.");
            Assert.That(GameObject.Find("Package_3"), Is.Not.Null,
                "The reciprocal package blocker must remain.");
            Assert.That(GameObject.Find("Package_4"), Is.Not.Null,
                "The package blocked by the fixed blocker must remain after repeated requests.");
            Assert.That(GameObject.Find("Blocker_5"), Is.Not.Null);
        }

        [UnityTest]
        public IEnumerator MousePress_SelectsValidPackage_AndRapidRepeatDoesNotDuplicateMove()
        {
            yield return LoadGameplayScene();

            PackageView blue = FindPackage(1);
            Camera camera = Camera.main;
            Assert.That(camera, Is.Not.Null);
            Assert.That(Object.FindFirstObjectByType<PackageInputController>(), Is.Not.Null);

            Mouse mouse = InputSystem.AddDevice<Mouse>();
            try
            {
                Vector2 screenPosition = camera.WorldToScreenPoint(blue.transform.position);

                InputSystem.QueueStateEvent(mouse, new MouseState
                {
                    position = screenPosition,
                    buttons = 1
                });
                yield return null;

                InputSystem.QueueStateEvent(mouse, new MouseState
                {
                    position = screenPosition,
                    buttons = 0
                });
                yield return null;

                InputSystem.QueueStateEvent(mouse, new MouseState
                {
                    position = screenPosition,
                    buttons = 1
                });
                yield return null;

                yield return new WaitForSeconds(0.7f);

                Assert.That(GameObject.Find("Package_1"), Is.Null,
                    "A rapid repeated mouse press must not execute the valid move twice.");
            }
            finally
            {
                InputSystem.RemoveDevice(mouse);
            }
        }

        [UnityTest]
        public IEnumerator TouchPress_SelectsPackages_AndRepeatedBlockedPressesRemainSafe()
        {
            yield return LoadGameplayScene();

            PackageView blue = FindPackage(1);
            PackageView yellow = FindPackage(4);
            Camera camera = Camera.main;
            Assert.That(camera, Is.Not.Null);

            Touchscreen touchscreen = InputSystem.AddDevice<Touchscreen>();
            try
            {
                yield return TouchPackage(touchscreen, camera, blue, 1);
                yield return new WaitForSeconds(0.7f);

                Assert.That(GameObject.Find("Package_1"), Is.Null,
                    "A touch press must select and resolve the valid package.");

                yield return TouchPackage(touchscreen, camera, yellow, 2);
                yield return TouchPackage(touchscreen, camera, yellow, 3);
                yield return new WaitForSeconds(0.5f);

                Assert.That(GameObject.Find("Package_4"), Is.Not.Null,
                    "Repeated touch presses must not remove a blocked package.");
                Assert.That(GameObject.Find("Blocker_5"), Is.Not.Null);
            }
            finally
            {
                InputSystem.RemoveDevice(touchscreen);
            }
        }

        private static IEnumerator LoadGameplayScene()
        {
            yield return SceneManager.LoadSceneAsync("Gameplay");
            yield return null;
        }

        private static IEnumerator TouchPackage(
            Touchscreen touchscreen,
            Camera camera,
            PackageView packageView,
            int touchId)
        {
            Vector2 screenPosition = camera.WorldToScreenPoint(packageView.transform.position);

            InputSystem.QueueStateEvent(touchscreen, new TouchState
            {
                touchId = touchId,
                phase = UnityEngine.InputSystem.TouchPhase.Began,
                position = screenPosition
            });
            yield return null;

            InputSystem.QueueStateEvent(touchscreen, new TouchState
            {
                touchId = touchId,
                phase = UnityEngine.InputSystem.TouchPhase.Ended,
                position = screenPosition
            });
            yield return null;
        }

        private static PackageView FindPackage(int packageId)
        {
            GameObject packageObject = GameObject.Find($"Package_{packageId}");
            Assert.That(packageObject, Is.Not.Null, $"Package_{packageId} was not spawned.");

            PackageView view = packageObject.GetComponent<PackageView>();
            Assert.That(view, Is.Not.Null);
            return view;
        }
    }
}
