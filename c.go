package main

import (
	"fmt"
	"math/rand"
	"time"
	"sync"
)


var graph = [][]int{}
var rm_lock sync.RWMutex

var iteration_count = 10

var done = make(chan bool, 4)

func get_rand_city_pair() (int, int) {
	size := len(graph)

	first := rand.Intn(size)
			
	second := rand.Intn(size)
	for second == first {
		second = rand.Intn(size)
	}

	return first, second
}

func change_price() {
	for i := 0; i < iteration_count; i++ {
		rm_lock.Lock()

		size := len(graph)

		if size >= 2 {
			first , second := get_rand_city_pair()
			
			if graph[first][second] != 0 {
				new_weight := rand.Intn(100) + 1

				graph[first][second] = new_weight
				graph[second][first] = new_weight
			}
		}

		rm_lock.Unlock()

		time.Sleep(10 * time.Millisecond)
	}

	done <- true
}

func add_remove_road() {
	for i := 0; i < iteration_count; i++ {
		rm_lock.Lock()

		size := len(graph)

		if size >= 2 {
			first , second := get_rand_city_pair()

			if rand.Intn(1) == 0 {
				new_weight := rand.Intn(100) + 1

				graph[first][second] = new_weight
				graph[second][first] = new_weight
			} else {
				graph[first][second] = 0
				graph[second][first] = 0
			}
		}
		rm_lock.Unlock()

		time.Sleep(10 * time.Millisecond)
	}

	done <- true
}

func add_remove_city() {
	for i := 0; i < iteration_count; i++ {
		rm_lock.Lock()

		size := len(graph)

		if rand.Intn(4) == 0 && size > 0 {
			size--
			graph = graph[:size]
			for i := 0; i < size; i++ {
				graph[i] = graph[i][:size]
			}
		} else {
			size++
			graph = append(graph, make([]int, size))
			for i := 0; i < size - 1; i++ {
				graph[i] = append(graph[i], 0)
			}
		}

		rm_lock.Unlock()

		time.Sleep(10 * time.Millisecond)
	}

	done <- true
}

func get_road_price(end int, current int, visit [][]bool) int {
	for i := 0; i < len(graph); i++ {
		vertex := graph[i][current]
		if vertex != 0 && !visit[i][current] {
			visit[i][current] = true

			if i == end {
				return graph[end][current]
			}

			price := get_road_price(end, i, visit)

			if price != -1 {
				return price + vertex
			}
		}
	}
	return -1
}

func get_road() {
	for i := 0; i < iteration_count; i++ {
		rm_lock.RLock()

		size := len(graph)

		if size >= 2 {
			first , second := get_rand_city_pair()

			visit := make([][]bool, size)
			for j := 0; j < size; j++ {
				visit[j] = make([]bool, size)
			}

			price := get_road_price(first, second, visit)

			if price == -1 {
				fmt.Printf("From %d to %d no way\n", first, second)
			} else {
				fmt.Printf("From %d to %d price is %d\n", first, second, price)
			}
		}
		

		rm_lock.RUnlock()

		time.Sleep(10 * time.Millisecond)
	}

	done <- true
}


func main() {
	go change_price()
	go add_remove_road()
	go add_remove_city()
	go get_road()

	<-done
	<-done
	<-done
	<-done
}
