# ModelMapper
An easy to use mapping helper which accepts different lamda expressions and helps in copying values from one type to another type based on the mapped members.
There are 3 such helpers.
1. **ModelMapper** - Which is the standard mapper based on expression generation and mapping using that function.
1. **ModelMapperLite** - Which is another standard mapper mostly used based on expression and reflection.
2. **ModelMapperBiDir** - It allows mapping between two types and allows definiting a conversion helper to help with mapping properties of different data type.
3. **ModelMapperUnary** - It allows mapping between same types. Its best use case is a copy constructor which makes deep copy where you dont have to state the mapping explicitly.

## Performance and uses
1. **ModelMapper** should serve general use cases and performs much better for mapping above 7000 objects with average amount of properties to map than the lite version.
2. **ModelMapper** should similar purposes as its **ModelMapper** counter-part and specifically mapping below 7000 objects based on similar parameters as mentioned above. But above this number the performance impacts are considerable.
3. **ModelMapperBiDir** is a bidirectional mapper used to map two objects of different types from first to second as well as second to first without having to define the mapping separately. For now it still works on reflection so larger data sets could face performance impact.
4. **ModelMapperUnary** is for a single type and does not need to have mapping specified between members can can be used for cloning an object.

## Motivation 
Automapper in .NET. Its slightly a bit complex to define mapping although pretty rich in features. The goal here was on the basic use cases and to make defining mapping as simple as possible.

## Notes
The performance may not be very impressive considering many things can be improved like caching of the compiled converters etc. But pretty usable and serves basic usecases easily.
