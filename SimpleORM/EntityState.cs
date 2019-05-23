namespace SimpleORM
{
    /// <summary>
    /// Stany encji
    /// </summary>
    public enum EntityState
    {
        /// <summary>
        /// Encja została powiązana z ORM lecz nie trafiła jeszcze do bazy danych
        /// </summary>
        Added = 1,

        /// <summary>
        /// Encja została zmodyfikowana, a modyfikacje nie trafiły do bazy danych
        /// </summary>
        Modified = 2,

        /// <summary>
        /// Encja nie jest powiązana z ORM
        /// </summary>
        Detached = 3,

        /// <summary>
        /// Encja oczekuje na usunięcie z bazy danych. Po usunięciu nie będzie powiązana z ORM
        /// </summary>
        Deleted = 4,

        /// <summary>
        /// Encja jest powiązana z ORM, a jej stan nie wymaga dokonania modyfikacji bazy danych.
        /// </summary>
        Unchanged = 5
    }
}