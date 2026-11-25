using UnityEngine;

/// <summary>
/// Безопасные расширения для Transform, предотвращающие ArgumentNullException при установке parent
/// </summary>
public static class SafeTransformExtensions
{
    /// <summary>
    /// Безопасно устанавливает parent для Transform. Если parent равен null, метод просто возвращается без ошибки.
    /// </summary>
    public static void SafeSetParent(this Transform transform, Transform parent, bool worldPositionStays = true)
    {
        // Если transform null, ничего не делаем
        if (transform == null) return;

        // Если parent null, ничего не делаем - НЕ вызываем SetParent
        if (parent == null) return;

        // Дополнительная проверка через ReferenceEquals для надежности
        if (ReferenceEquals(parent, null)) return;

        // Только если все проверки пройдены, вызываем SetParent
        try
        {
            transform.SetParent(parent, worldPositionStays);
        }
        catch
        {
            // Игнорируем все исключения - метод должен быть безопасным
        }
    }
}

