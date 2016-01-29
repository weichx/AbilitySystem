using System;

[AttributeUsage(AttributeTargets.Method)]
public class StatusAttribute : Attribute { }
public class OnStatusAppliedHandler : StatusAttribute {

    public OnStatusAppliedHandler(Type attachable) {

    }

}

public class OnStatusUpdatedHandler : StatusAttribute { }
public class OnStatusRefreshedHandler : StatusAttribute { }
public class OnStatusDispelledHandler : StatusAttribute { }
public class OnStatusRemovedHandler : StatusAttribute { }
public class OnStatusExpiredHandler : StatusAttribute { }