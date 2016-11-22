using System;
using NUnit.Framework;
using UnityEngine;

namespace Spec {

    internal class SomeType {}

    internal class SomeExtendedType : SomeType {}

    [PropertyDrawerFor(typeof(SomeType))]
    class SomeDrawerX : PropertyDrawerX {

        public override void OnGUI(SerializedPropertyX property, GUIContent label) {
            throw new NotImplementedException();
        }

    }

    [TestFixture]
    [Category("Reflector")]
    internal class ReflectorSpec {

        [Test]
        public void FindCustomPropertyDrawerForSameType() {
            SerializedPropertyX property = new SerializedPropertyX("name", typeof(SomeType), new SomeType());
            PropertyDrawerX drawerX = Reflector.GetCustomPropertyDrawerFor(property);
            Assert.IsNotNull(drawerX);
            Assert.IsInstanceOf<SomeDrawerX>(drawerX);
        }

        [Test]
        public void FindCustomPropertyDrawerForSubclass() {
            SerializedPropertyX property = new SerializedPropertyX("name", typeof(SomeType), new SomeType());
            PropertyDrawerX drawerX = Reflector.GetCustomPropertyDrawerFor(property);
            Assert.IsNotNull(drawerX);
            Assert.IsInstanceOf<SomeDrawerX>(drawerX);
        }

        [Test]
        public void FindTheSamePropertyDrawerOnSecondCall() {
            SerializedPropertyX property = new SerializedPropertyX("name", typeof(SomeType), new SomeType());
            PropertyDrawerX drawer1 = Reflector.GetCustomPropertyDrawerFor(property);
            PropertyDrawerX drawer2 = Reflector.GetCustomPropertyDrawerFor(property);
            Assert.AreEqual(drawer1, drawer2);
        }
    }

}