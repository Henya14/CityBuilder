using System.Collections.Generic;

public interface ICarFactory<T>
{
    CarNavigation SpawnCar(List<GraphSearchNode<T>> route);
}