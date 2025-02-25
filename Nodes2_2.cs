namespace Nodes.Task2_2;

public static class Solution
{
    public static int CountIslands(char[][]? grid)
    {
        if (grid == null || grid.Length == 0 || grid[0].Length == 0) return 0;
        
        int islands = 0;
        var dirs = new (int, int)[] { (0, 1), (1, 0), (0, -1), (-1, 0) };
        
        for (int y = 0; y < grid.Length; y++)
        {
            for (int x = 0; x < grid[0].Length; x++)
            {
                if (grid[y][x] != '1') continue;
                
                islands++;
                FloodFill(grid, y, x, dirs);
            }
        }
        
        return islands;
    }
    
    private static void FloodFill(char[][] map, int row, int col, (int dy, int dx)[] directions)
    {
        var stack = new Stack<(int, int)>();
        stack.Push((row, col));
        map[row][col] = '0';

        while (stack.Count > 0)
        {
            var (r, c) = stack.Pop();
            
            foreach (var (dy, dx) in directions)
            {
                int ny = r + dy;
                int nx = c + dx;
                
                if (IsValidCell(map, ny, nx))
                {
                    map[ny][nx] = '0';
                    stack.Push((ny, nx));
                }
            }
        }
    }

    private static bool IsValidCell(char[][] map, int ny, int nx) =>
        ny >= 0 && ny < map.Length &&
        nx >= 0 && nx < map[0].Length &&
        map[ny][nx] == '1';
}


public static class App
{
    private static void Main()
    {
        char[][] grid =
        [
            ['1', '1', '1', '1', '0'],
            ['1', '1', '0', '1', '0'],
            ['1', '1', '0', '0', '0'],
            ['0', '0', '0', '0', '0']
        ];
        Console.WriteLine(Solution.CountIslands(grid)); // 1

        grid =
        [
            ['1', '1', '0', '0', '0'],
            ['1', '1', '0', '0', '0'],
            ['0', '0', '1', '0', '0'],
            ['0', '0', '0', '1', '1']
        ];
        Console.WriteLine(Solution.CountIslands(grid)); // 3
    }
}