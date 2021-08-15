# ModelMapper
An easy to use mapping helper which accepts different lamda expressions and helps in copying values from one type to another type based on the mapped members.
There are 3 such helpers.
1. **ModelMapper** - Which is the standard mapper mostly used based on one source type with a specific destination type.
2. **ModelMapperBiDir** - It allows mapping between two types and allows definiting a conversion helper to help with mapping properties of different data type.
3. **ModelMapperUnary** - It allows mapping between same types. Its best use case is a copy constructor which makes deep copy where you dont have to state the mapping explicitly.

## Motivation 
Automapper in .NET. Its slightly a bit complex to define mapping although pretty rich in features. The goal here was on the basic use cases and to make defining mapping as simple as possible.

## Notes
The performance may not be very impressive considering many things can be improved like caching of the compiled converters etc. But pretty usable and serves basic usecases easily.
