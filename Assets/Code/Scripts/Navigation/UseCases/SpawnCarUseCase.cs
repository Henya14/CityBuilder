public class SpawnCarUseCase<T>
{
    private readonly ICarFactory<T> _carFactory;

    public SpawnCarUseCase(ICarFactory<T> carFactory)
    {
        _carFactory = carFactory;
    }

    public CarNavigation Execute(FindRouteResponse<T> request)
    {
        if (!request.IsSuccess)
        {
            return null;
        }
        return _carFactory.SpawnCar(request.RouteResult.ShortestPath);
    }
}