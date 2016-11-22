using System;
using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

namespace Spec {

    [TestFixture]
    internal class SerializedPropertyXSpec {

        internal class TestThing {
            public int x;
            protected int y;
            private int z;
        }

        internal class TestThingSubclass : TestThing { }

        internal class TestThingSerialized {
            public int x;
            [SerializeField]
            protected int y;
            [SerializeField]
            private int z;
        }

        internal class TestThingNonSerialized {
            public int x;
            [NonSerialized]
            protected int y;
            [SerializeField]
            private int z;
        }

        internal class TestThingContainer {
            public TestThing thing;
        }

        [Test]
        public void CreatesInstanceIfInputNullAndEmptyConstructor() {
            SerializedPropertyX property = new SerializedPropertyX("name", typeof(TestThing));
            Assert.IsNotNull(property.Value);
            Assert.IsInstanceOf<TestThing>(property.Value);
        }

        [Test]
        public void CreatesStructInstanceIfValueNull() {
            SerializedPropertyX property = new SerializedPropertyX("name", typeof(Vector3));
            Assert.IsNotNull(property.Value);
            Assert.IsInstanceOf<Vector3>(property.Value);
        }

        [Test]
        public void CreatesPropertiesForPublicChildren() {
            SerializedPropertyX property = new SerializedPropertyX("name", typeof(TestThing));
            Assert.AreEqual(property.ChildCount, 1);
        }

        [Test]
        public void DoesNotCreatePropertiesForPrivateOrProtectedChildren() {
            SerializedPropertyX property = new SerializedPropertyX("name", typeof(TestThing));
            Assert.AreEqual(property.ChildCount, 1);
            Assert.IsNull(property["y"]);
            Assert.IsNull(property["z"]);
        }

        [Test]
        public void CreatePropertiesForPrivateOrProtectedChildrenWithSerializeFieldAttr() {
            SerializedPropertyX property = new SerializedPropertyX("name", typeof(TestThingSerialized));
            Assert.AreEqual(property.ChildCount, 3);
            Assert.IsNotNull(property["x"]);
            Assert.IsNotNull(property["y"]);
            Assert.IsNotNull(property["z"]);
        }

        [Test]
        public void DoesNotCreateChildrenIfNonSerializedAttrIsPresent() {
            SerializedPropertyX property = new SerializedPropertyX("name", typeof(TestThingNonSerialized));
            Assert.AreEqual(property.ChildCount, 2);
            Assert.IsNotNull(property["x"]);
            Assert.IsNotNull(property["z"]);
        }

        [Test]
        public void HasProperTypeWhenValueNull() {
            SerializedPropertyX property = new SerializedPropertyX("name", typeof(TestThing));
            property.Value = null;
            Assert.IsTrue(property.Type == typeof(TestThing));
        }

        [Test]
        public void HasProperTypeWhenValueNotNull() {
            SerializedPropertyX property = new SerializedPropertyX("name", typeof(TestThing));
            property.Value = new TestThing();
            Assert.IsTrue(property.Type == typeof(TestThing));
        }

        [Test]
        public void HasProperTypeWhenValueNotNullAndSubClass() {
            SerializedPropertyX property = new SerializedPropertyX("name", typeof(TestThingSubclass));
            property.Value = new TestThingSubclass();
            Assert.IsTrue(property.Type == typeof(TestThingSubclass));
        }

        [Test]
        public void ValueCanBeSet() {
            TestThing thing1 = new TestThing();
            TestThing thing2 = new TestThing();
            SerializedPropertyX property = new SerializedPropertyX("name", typeof(TestThing), thing1);
            Assert.AreEqual(property.Value, thing1);
            property.Value = thing2;
            Assert.AreEqual(property.Value, thing2);
        }

        [Test]
        public void ChangedIsSetWhenUpdatingValue() {
            TestThing thing1 = new TestThing();
            TestThing thing2 = new TestThing();
            SerializedPropertyX property = new SerializedPropertyX("name", typeof(TestThing), thing1);
            Assert.AreEqual(property.Value, thing1);
            Assert.IsFalse(property.Changed);
            property.Value = thing2;
            Assert.AreEqual(property.Value, thing2);
            Assert.IsTrue(property.Changed);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException), ExpectedMessage = "Unable to assign Int32 to TestThing")]
        public void ThrowsWhenValueAssignedIncompatible() {
            SerializedPropertyX property = new SerializedPropertyX("name", typeof(TestThing));
            property.Value = 1;
        }

        [Test]
        public void CanHandleArrayType() {
            SerializedPropertyX property = new SerializedPropertyX("name", typeof(string[]));
            property.Value = new string[] { "hello", "there" };
            Assert.IsInstanceOf<string[]>(property.Value);
            Assert.AreEqual(property.GetValue<string[]>()[0], "hello");
            Assert.AreEqual(property.GetValue<string[]>()[1], "there");
            Assert.AreEqual(property.ArraySize, 2);
            Assert.IsTrue(property.IsArrayLike);
        }

        [Test]
        public void CanSwapArrayElements() {
            SerializedPropertyX property = new SerializedPropertyX("name", typeof(string[]));
            property.Value = new string[] { "hello", "there" };
            var child1 = property.GetChildAt(0);
            var child2 = property.GetChildAt(1);

            property.SwapArrayElements(0, 1);

            Assert.AreEqual(property.GetValue<string[]>()[1], "hello");
            Assert.AreEqual(property.GetValue<string[]>()[0], "there");
            Assert.AreEqual(property.GetChildAt(1), child1);
            Assert.AreEqual(property.GetChildAt(0), child2);

            Assert.AreEqual(property.ChildCount, 2);
        }

        [Test]
        public void CanDeleteArrayElement() {
            SerializedPropertyX property = new SerializedPropertyX("name", typeof(string[]));
            property.Value = new string[] { "hello", "there" };
            var child1 = property.GetChildAt(1);
            property.DeleteArrayElementAt(0);
            string[] value = property.Value as string[];
            Assert.AreEqual(value[0], "there");
            Assert.AreEqual(child1, property.GetChildAt(0));
            Assert.AreEqual(property.ChildCount, 1);
        }

        [Test]
        public void CanCreateArray() {
            SerializedPropertyX property = new SerializedPropertyX("name", typeof(string[]));
            Assert.IsNotNull(property.Value);
            Assert.IsInstanceOf<string[]>(property.Value);
            Assert.AreEqual(property.ChildCount, 0);
        }

        [Test]
        public void CanResizeArrayToLarger() {
            SerializedPropertyX property = new SerializedPropertyX("name", typeof(string[]), new string[] { "1", "2" });
            property.ArraySize++;
            Assert.AreEqual(property.GetValue<string[]>().Length, 3);
            Assert.AreEqual(property.ChildCount, 3);
        }

        [Test]
        public void CanResizeArrayToSmaller() {
            SerializedPropertyX property = new SerializedPropertyX("name", typeof(string[]), new string[] { "1", "2" });
            property.ArraySize--;
            Assert.AreEqual(property.GetValue<string[]>().Length, 1);
            Assert.AreEqual(property.GetValue<string[]>()[0], "1");
        }

        [Test]
        public void CanHandleListType() {
            SerializedPropertyX property = new SerializedPropertyX("name", typeof(List<string>));
            property.Value = new List<string> { "hello", "there" };
            Assert.IsInstanceOf<List<string>>(property.Value);
            Assert.AreEqual(property.GetValue<List<string>>()[0], "hello");
            Assert.AreEqual(property.GetValue<List<string>>()[1], "there");
            Assert.AreEqual(property.ArraySize, 2);
            Assert.IsTrue(property.IsArrayLike);
        }

        [Test]
        public void CanSwaListElements() {
            SerializedPropertyX property = new SerializedPropertyX("name", typeof(List<string>));
            property.Value = new List<string> { "hello", "there" };
            var child1 = property.GetChildAt(0);
            var child2 = property.GetChildAt(1);

            property.SwapArrayElements(0, 1);

            Assert.AreEqual(property.GetValue<List<string>>()[1], "hello");
            Assert.AreEqual(property.GetValue<List<string>>()[0], "there");
            Assert.AreEqual(property.GetChildAt(1), child1);
            Assert.AreEqual(property.GetChildAt(0), child2);

            Assert.AreEqual(property.ChildCount, 2);
        }

        [Test]
        public void CanDeleteListElement() {
            SerializedPropertyX property = new SerializedPropertyX("name", typeof(List<string>));
            property.Value = new List<string> { "hello", "there" };
            var child1 = property.GetChildAt(1);
            property.DeleteArrayElementAt(0);
            List<string> value = property.Value as List<string>;
            Assert.AreEqual(value[0], "there");
            Assert.AreEqual(child1, property.GetChildAt(0));
            Assert.AreEqual(property.ChildCount, 1);
        }

        [Test]
        public void CanCreateList() {
            SerializedPropertyX property = new SerializedPropertyX("name", typeof(List<string>));
            Assert.IsNotNull(property.Value);
            Assert.IsInstanceOf<List<string>>(property.Value);
            Assert.AreEqual(property.ChildCount, 0);
        }

        [Test]
        public void CanResizeListToLarger() {
            SerializedPropertyX property = new SerializedPropertyX("name", typeof(List<string>), new List<string> { "1", "2" });
            property.ArraySize++;
            Assert.AreEqual(property.GetValue<List<string>>().Count, 3);
            Assert.AreEqual(property.ChildCount, 3);
        }

        [Test]
        public void CanResizListToSmaller() {
            SerializedPropertyX property = new SerializedPropertyX("name", typeof(List<string>), new List<string> { "1", "2" });
            property.ArraySize--;
            Assert.AreEqual(property.GetValue<List<string>>().Count, 1);
            Assert.AreEqual(property.GetValue<List<string>>()[0], "1");
        }

        [Test]
        public void ChangeFlagIsFlippedWhenNewValueIsAssigned() {
            TestThing test1 = new TestThing();
            TestThing test2 = new TestThing();
            SerializedPropertyX property = new SerializedPropertyX("name", typeof(TestThing), test1);
            Assert.IsFalse(property.Changed);
            property.Value = test2;
            Assert.IsTrue(property.Changed);
        }

        [Test]
        public void ChangeFlagIsNotSetWhenSameReferenceValueAssigned() {
            TestThing test = new TestThing();
            SerializedPropertyX property = new SerializedPropertyX("name", typeof(TestThing), test);
            Assert.IsFalse(property.Changed);
            property.Value = test;
            Assert.IsFalse(property.Changed);
        }

        [Test]
        public void ChangeFlagIsNotSetWhenSameValueTypeValueAssigned() {
            SerializedPropertyX property = new SerializedPropertyX("name", typeof(float), 10);
            Assert.IsFalse(property.Changed);
            property.Value = 10;
            Assert.IsFalse(property.Changed);
        }

        [Test]
        public void ChangedFlagIsFlippedWhenManipulatingArray() {
            List<string> list = new List<string>();
            list.Add("Hello");
            list.Add("There");
            list.Add("Unity");
            SerializedPropertyX property = new SerializedPropertyX("name", typeof(List<string>));
            property.ArraySize++;
            Assert.IsTrue(property.Changed);

            property = new SerializedPropertyX("name", typeof(List<string>), list);
            property.ArraySize--;
            Assert.IsTrue(property.Changed);

            property = new SerializedPropertyX("name", typeof(List<string>), list);
            property.SwapArrayElements(1, -1);
            Assert.IsTrue(property.Changed);

            property = new SerializedPropertyX("name", typeof(List<string>), list);
            property.DeleteArrayElementAt(1);
            Assert.IsTrue(property.Changed);
        }

        [Test]
        public void ChangedIsClearedWhenApplyingModifiedProperties() {
            TestThingSerialized thing = new TestThingSerialized();
            SerializedPropertyX property = new SerializedPropertyX("name", typeof(TestThingSerialized), thing);
            property.Value = new TestThingSerialized();
            Assert.IsTrue(property.Changed);
            property.ApplyModifiedProperties();
            Assert.IsFalse(property.Changed);
        }

        [Test]
        public void ChangedIsSetWhenChildPropertyIsChanged() {
            TestThingSerialized thing = new TestThingSerialized();
            SerializedPropertyX property = new SerializedPropertyX("name", typeof(TestThingSerialized), thing);
            property["x"].Value = 10;
            Assert.IsTrue(property.Changed);
        }

        [Test]
        public void ChildrenProperlySetToOrphanedWhenValueSet() {
            SerializedPropertyX property = new SerializedPropertyX("name", typeof(TestThingContainer));
            List<SerializedPropertyX> children = new List<SerializedPropertyX>();
            for (int i = 0; i < property.ChildCount; i++) {
                children.Add(property.GetChildAt(i));
            }
            property.Value = new TestThingContainer();
            for (int i = 0; i < children.Count; i++) {
                Assert.IsTrue(children[i].IsOrphaned);
            }
            Assert.IsTrue(children.Count > 0);
        }

        [Test]
        public void ChildSetToOrphanedWhenDeletingArrayElement() {
            string[] strings = new string[] { "one", "two" };
            SerializedPropertyX property = new SerializedPropertyX("name", typeof(string[]), strings);
            SerializedPropertyX child = property.GetChildAt(1);
            Assert.IsFalse(child.IsOrphaned);
            property.DeleteArrayElementAt(1);
            Assert.IsTrue(child.IsOrphaned);
        }

        [Test]
        public void ChildrenSetToOrphanedWhenDecreasingArraySize() {
            string[] strings = new string[] { "one", "two" };
            SerializedPropertyX property = new SerializedPropertyX("name", typeof(string[]), strings);
            SerializedPropertyX child = property.GetChildAt(1);
            Assert.IsFalse(child.IsOrphaned);
            property.ArraySize--;
            Assert.IsTrue(child.IsOrphaned);
        }

        [Test]
        public void ChildrenSetToOrphanedWhenDecreasingListSize() {
            List<string> strings = new List<string>(new string[] {"one", "two"});
            SerializedPropertyX property = new SerializedPropertyX("name", typeof(List<string>), strings);
            SerializedPropertyX child = property.GetChildAt(1);
            Assert.IsFalse(child.IsOrphaned);
            property.ArraySize--;
            Assert.IsTrue(child.IsOrphaned);
        }

        internal class CircularBase {
            public CirclarChild child;
        }

        internal class CirclarChild {
            public CircularBase parent;
        }

        //[Test]
        //public void HandleCircularReference() {
        //   SerializedPropertyX property = new SerializedPropertyX("name", typeof(CircularBase));
        //    Assert.IsTrue(property["child"].IsCircular);
        //}

    }

}