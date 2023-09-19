using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;

namespace HorseGame
{
    internal class Program
    {
        static string stringInput(string caption)
        {
            Console.Write(caption);
            return Console.ReadLine();
        }
        static (int, string, bool) intInput(string caption)
        {
            int result = -1;
            bool parseError = true;
            string input_data;

            Console.Write(caption);
            input_data = Console.ReadLine();
            if (int.TryParse(input_data, out result))
            {
                parseError = false;
            }
            return (result, input_data, parseError);
        }

        static Coords getGameMapSize()
        {
            Coords map_size;

            while (true)
            {
                map_size = coordsInput("Введите высоту и ширину карты через пробел: ");
                if (map_size.is_empty)
                    Console.WriteLine("Координаты должны в формате (Число Число)!");
                else if (map_size.y < 3)
                    Console.WriteLine("Высота должны быть не меньше 3!");
                else if (map_size.x < 3)
                    Console.WriteLine("Ширина должны быть не меньше 3!");
                else
                    return map_size;
                Console.WriteLine();
            }
        }

        static GameMap createGameMap(Coords map_size, char border_char)
        {
            GameMap game_map = new GameMap(map_size, border_char);
            return game_map;
        }

        static Coords coordsInput(string caption)
        {
            string raw_input = Regex.Replace(stringInput(caption), "\\s+", " "); // Regex.Replace - заменяет символы по паттерну \s+ (Несколько пробелов) на " "
            raw_input = Regex.Match(raw_input, "((-)?\\d+ (-)?\\d+)").Value; // "((-)?\\d+ (-)?\\d+)" - текст в формате (- или + Число пробел - или + Число)
            if (raw_input != "")
            {
                string[] raw_coords = raw_input.Split(' ');
                Coords coords = new Coords(int.Parse(raw_coords[0]), int.Parse(raw_coords[1]), false);
                return coords;
            }
            Coords empty_coords = new Coords(0, 0, true);
            return empty_coords;
        }

        static Coords coordsInputWhile(GameMap game_map, string caption, bool autoclear=true, bool autoview=true)
        {
            Coords coords;
            while (true)
            {
                if(autoclear)
                    Console.Clear();
                if(autoview)
                    game_map.view();
                Console.WriteLine();
                coords = coordsInput(caption);
                if (coords.is_empty)
                { 
                    Console.WriteLine("Координаты должны в формате (Число Число)!");
                    autoclear = false;
                    autoview = false;
                }
                else if (!game_map.coordsInMap(coords))
                { 
                    Console.WriteLine("Координаты вышли за границы карты!");
                    autoclear = false;
                    autoview = false;
                }
                else
                    break;
            }
            return coords;
        }

        static (int, Player) createPlayer(int player_index, KeyValuePair<char, bool>[] player_chars, GameMap game_map)
        {
            Player player;
            Coords start_position;
            int horse_index;
            bool parseError;

            string name = stringInput(String.Format("Как будем звать игрока под номером {0}?: ", player_index));
            Console.WriteLine("Теперь время выбрать коня!");
            Console.WriteLine("От список доступных коней:");
            for (int i = 0; i < player_chars.Length; i++)
            {
                if (!player_chars[i].Value)
                    Console.WriteLine("    {0} - '{1}'", i + 1, player_chars[i].Key);
            }
            while (true)
            {
                (horse_index, _, parseError) = intInput("Введи номер коня из списка: ");
                if (horse_index <= 0 | horse_index > player_chars.Length | parseError)
                    Console.WriteLine("Вы ввели не правильный номер коня!");
                else if (player_chars[horse_index - 1].Value)
                    Console.WriteLine("Конь под этим номер уже занят!");
                else
                    break;
            }

            Console.WriteLine("Отлично! Время выбрать начальную позицию коня!");
            while (true)
            {
                start_position = coordsInputWhile(game_map, "Введи позицию коня в формате (Число Число): ");
                if (!game_map.coordsInMap(start_position))
                    Console.WriteLine("Координаты вышли за границы карты!");
                else if (game_map.get(start_position) != ' ')
                {
                    Console.WriteLine("Эта точка занята другим игроком!");
                    Console.Write("Нажми Enter чтобы продолжить...");
                    Console.ReadKey();
                }
                else
                    break;
            }
            player = new Player(name, player_chars[horse_index - 1].Key, start_position);
            return (horse_index - 1, player);
        }

        struct Coords
        {
            public int y;
            public int x;
            public bool is_empty;

            public Coords(int y, int x, bool is_empty)
            {
                this.y = y;
                this.x = x;
                this.is_empty = is_empty;
            }
        }

        struct GameMap
        {
            private Coords size;
            private char[,] map;
            private char border_char;

            public GameMap(Coords map_size, char border_char)
            {
                this.border_char = border_char;
                this.size = map_size;
                this.map = new char[this.size.y, this.size.x];
                for (int iy = 0; iy < this.size.y; iy++)
                {
                    for (int ix = 0; ix < this.size.x; ix++)
                    {
                        this.map[iy, ix] = ' ';
                    }
                }
            }

            public bool coordsInMap(Coords coords)
            {
                if ( (coords.x < 0 | coords.x > this.size.x-1) | (coords.y < 0 | coords.y > this.size.y-1) )
                    return false;
                return true;
            }

            public Coords getSize()
            {
                return this.size;
            }

            public int getSizeX()
            {
                return this.size.x;
            }

            public int getSizeY()
            {
                return this.size.y;
            }

            public void view()
            {
                string head_bord;
                string inside_line;

                Console.Write("   ");
                for (int i = 0; i < this.size.x; i++)
                {
                    if (i > 9)
                        Console.Write(i.ToString() + " ");
                    else
                        Console.Write(i.ToString() + "  ");
                }
                Console.WriteLine();

                head_bord = "   " + string.Concat(Enumerable.Repeat(this.border_char.ToString() + "  ", this.size.x));

                Console.WriteLine(head_bord);
                for (int iy = 0; iy < this.size.y; iy++)
                {
                    Console.Write(this.border_char.ToString() + ' ');
                    for (int ix = 0; ix < this.size.x; ix++)
                    {
                        inside_line = String.Format("|{0}|", this.map[iy, ix]);
                        Console.Write(inside_line);
                    }
                    Console.WriteLine(' ' + this.border_char.ToString() + ' ' + iy.ToString());
                }
                Console.WriteLine(head_bord);
            }

            public bool edit(Coords position, char new_char)
            {
                if (position.is_empty | position.x < 0 | position.x > this.size.x - 1 | position.y < 0 | position.y > this.size.y - 1)
                    return false;
                this.map[position.y, position.x] = new_char;
                return true;
            }

            public bool edit(int y, int x, char new_char)
            {
                if (x < 0 | x > this.size.x - 1 | y < 0 | y > this.size.y - 1)
                    return false;
                this.map[y, x] = new_char;
                return true;
            }

            public char get(Coords position)
            {
                if (this.coordsInMap(position))
                    return this.map[position.y, position.x];
                return '\0';
            }
        }

        struct Player
        {
            private string name;
            private char char_texture;
            private Coords position;

            public Player(string name, char char_texture, Coords start_position)
            {
                this.name = name;
                this.char_texture = char_texture;
                this.position = start_position;
            }

            public Coords getPosition()
            {
                return this.position;
            }

            public void setPosition(Coords new_position)
            {
                this.position = new_position;
            }

            public string getName()
            {
                return this.name;
            }

            public char getCharTexture()
            {
                return this.char_texture;
            }

            public Coords[] getNextExistSteps(GameMap game_map, Player next_players)
            {
                KeyValuePair<string, int[]>[] steps = {
                    new KeyValuePair<string, int[]>("rrstep", new int[]{ 1, 2 }),
                    new KeyValuePair<string, int[]>("rlstep", new int[]{ -1, 2 }),

                    new KeyValuePair<string, int[]>("lrstep", new int[]{ 1, -2 }),
                    new KeyValuePair<string, int[]>("llstep", new int[]{ -1, -2 }),

                    new KeyValuePair<string, int[]>("urstep", new int[]{ -2, 1 }),
                    new KeyValuePair<string, int[]>("ulstep", new int[]{ -2, -1 }),

                    new KeyValuePair<string, int[]>("drstep", new int[]{ 2, 1 }),
                    new KeyValuePair<string, int[]>("dlstep", new int[]{ 2, -1 })
                };

                List<Coords> exists_coords_list = new List<Coords>();
                int[] offset;
                for (int i = 0; i < steps.Length; i++)
                {
                    offset = steps[i].Value;
                    Coords offset_coords = new Coords(this.position.y + offset[0], this.position.x+offset[1], true);
                    if (game_map.coordsInMap(offset_coords) && game_map.get(offset_coords) != '#')
                    {
                        Coords next_players_coords = next_players.getPosition();
                        if (next_players_coords.x != offset_coords.x | next_players_coords.y != offset_coords.y)
                            exists_coords_list.Add(offset_coords);
                    }
                }
                return exists_coords_list.ToArray();
            }
        }

        static void Main(string[] args)
        {

            /* Неизменяемые игровые символы-текстуры
             * ' ' - Свободная клетка
             * '#' - Использованая клетка
            */

            /* Изменяемые игровые символы-текстуры
             * '-' - Граница карты (Изменяется по аргументу border_char в функции createGameMap)
            */

            // Создание карты
            GameMap game_map = createGameMap(getGameMapSize(), '*');
            Console.WriteLine("Размер карты (Высота: {0} | Ширина: {1})\n", game_map.getSizeY(), game_map.getSizeX());

            // Инициализация 2 игроков
            Player[] players = new Player[2];

            KeyValuePair<char, bool>[] player_chars = { // Сюда можно добавить новые символы для выбора, кроме ' ', '#', border_char!!!
                new KeyValuePair<char, bool>(',', false),
                new KeyValuePair<char, bool>('.', false)
            };

            (int index_used_char, Player player) = createPlayer(1, player_chars, game_map);
            game_map.edit(player.getPosition(), player.getCharTexture());
            player_chars[index_used_char] = new KeyValuePair<char, bool>(player_chars[index_used_char].Key, true);
            players[0] = player;

            Console.Clear();

            (index_used_char, player) = createPlayer(2, player_chars, game_map);
            game_map.edit(player.getPosition(), player.getCharTexture());
            players[1] = player;

            // Цикл игры
            bool first_player = true;
            int ii;
            Coords[] nesteps;
            Coords new_position;
            bool is_changed;

            while (true)
            {
                Console.Clear();
                game_map.view();
                is_changed = false;
                first_player = !first_player;
                player = players[Convert.ToInt32(first_player)];
                nesteps = player.getNextExistSteps(game_map, players[Convert.ToInt32(!first_player)]);
                if (nesteps.Length > 0)
                {
                    Console.WriteLine("\nВам доступны от такие шаги:");
                    for (ii = 0; ii < nesteps.Length; ii++)
                        Console.WriteLine(String.Format("    {0} - {1} {2}", ii + 1, nesteps[ii].y, nesteps[ii].x));
                    while (true)
                    {
                        new_position = coordsInputWhile(game_map, String.Format("Игрок под номером {0} с именем {1}, ходи своим конём! Введи координаты: ", Convert.ToInt32(first_player)+1, player.getName()), false, false);
                        for (ii = 0; ii < nesteps.Length; ii++)
                        {
                            if (new_position.x == nesteps[ii].x && new_position.y == nesteps[ii].y)
                            {
                                game_map.edit(player.getPosition(), '#');
                                player.setPosition(new_position);
                                players[Convert.ToInt32(first_player)] = player;
                                game_map.edit(new_position, player.getCharTexture());
                                is_changed = true;
                                break;
                            }
                        }
                        if (is_changed)
                            break;
                        else
                        {
                            Console.WriteLine("Ваши координаты не соответствуют правилам игры!");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("-- У {0} не осталось шагов!!! --", player.getName());
                    Console.WriteLine("-- {0} ВЫГРАЛ!!! --", players[Convert.ToInt32(!first_player)].getName());
                    Console.Write("Нажми Enter чтобы выйти...");
                    Console.ReadKey();
                    break;
                }
            }
        }
    }
}
