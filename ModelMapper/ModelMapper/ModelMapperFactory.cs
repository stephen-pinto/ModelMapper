using ModelMapper.Basic;

namespace ModelMapper
{
    public enum ModelDataSize
    {
        Small,
        Large
    }

    public static class ModelMapperFactory
    {
        public static IModelMapper<SrcType, DstType> GetModelMapper<SrcType, DstType>(ModelDataSize size = ModelDataSize.Large)
            where SrcType: class
            where DstType: class
        {
            switch (size)
            {
                case ModelDataSize.Small:
                    return new ModelMapperLite<SrcType, DstType>();
                case ModelDataSize.Large:
                default:
                    return new ModelMapper<SrcType, DstType>();
            }
        }
    }
}
