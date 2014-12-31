using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities.StructureMapperExtension
{
    public class DecoratedInstance<TTarget>
    {
        public DecoratedInstance<TTarget> AndThen<TDecorator>()
        {
            //Must be captured as a local variable, otherwise the closure's decorator will
            //always be the outer-most decorator, causing a Stack Overflow.

            //var previousDecorator = _decorator;
 
            //ContextEnrichmentHandler<TTarget> newDecorator = (ctx, t) =>
            //    {
            //        var pluginType = ctx.BuildStack.Current.RequestedType;
 
            //        var innerInstance = previousDecorator(ctx, t);
 
            //        ctx.RegisterDefault(pluginType, innerInstance);
 
            //        return ctx.GetInstance<TDecorator>();
            //    };
 
            //    _instance.EnrichWith(newDecorator);
 
            //    _decorator = newDecorator;
 
                return this;
        }
    }
}
