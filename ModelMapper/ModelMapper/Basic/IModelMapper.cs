using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ModelMapper.Basic
{
    public interface IModelMapper<SrcType, DstType>
        where SrcType : class
        where DstType : class
    {
        IModelMapper<SrcType, DstType> Add<ResType>(Expression<Func<ResType>> source, Expression<Func<DstType, ResType>> destination);

        IModelMapper<SrcType, DstType> Add<ResType>(Expression<Func<SrcType, ResType>> source, Expression<Func<DstType, ResType>> destination);

        IModelMapper<SrcType, DstType> Add<ResType>(Expression<Func<ResType>> source, Expression<Func<DstType, ResType>> destination, ResType defaultValue)
            where ResType : class;

        IModelMapper<SrcType, DstType> Add<ResType>(Expression<Func<SrcType, ResType>> source, Expression<Func<DstType, ResType>> destination, ResType defaultValue)
            where ResType : class;

        IModelMapper<SrcType, DstType> Build();

        void Clear();

        void CopyChanges(SrcType sourceObj, DstType destinationObj);

        DstType GetNew(SrcType sourceObj);
    }
}
