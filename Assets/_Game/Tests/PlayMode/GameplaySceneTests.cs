using System.Collections;
using NUnit.Framework;
using ParcelEscape.Gameplay;
using UnityEngine;
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
