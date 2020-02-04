package main

import "fmt"

func get_winner(respond chan<- int, index int, data []int) {
	size := len(data)

	if index >= size / 2 {
		result := 0
		if data[2 * index - size] >= data[2 * index + 1 - size] {
			result = 2 * index	
		} else {
			result = 2 * index + 1
		}

		respond <- result
	} else {
		current_respond := make(chan int, 2)

		go get_winner(current_respond, 2 * index, data)
		go get_winner(current_respond, 2 * index + 1, data)
		
		firstData := <-current_respond
		secondData := <-current_respond

		result := 0

		if data[firstData - size] >= data[secondData - size] {
			result = firstData
		} else {
			result = secondData
		}

		respond <- result
	}
}

func main() {
	data := []int{ 12, 3, 5, 13, 4, 1, 13, 20, 22, 3, 5, 17, 1, 1, 13, 20}

	respond := make(chan int, 2)
	go get_winner(respond, 1, data)

	result := <-respond

	fmt.Printf("Result = %d\n", data[result - len(data)])
}