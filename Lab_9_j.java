import java.util.Random;
import java.util.concurrent.ForkJoinPool;
import java.util.concurrent.ForkJoinTask;


class MatrixMultiplication {
	private final int size = 1000;
	private final int worldSize = 4;

	private float[] a = new float[size * size];
	private float[] b = new float[size * size];
	private float[] c = new float[size * size];

	private Random random = new Random();

	public MatrixMultiplication() {
		for (int i = 0; i < size; ++i) {
			for (int j = 0; j < size; ++j) {
				a[i * size + j] = (random.nextFloat() - 0.5f) * 50.0f;
			}
		}
		for (int i = 0; i < size; ++i) {
			for (int j = 0; j < size; ++j) {
				b[i * size + j] = (random.nextFloat() - 0.5f) * 50.0f;
			}
		}
		for (int i = 0; i < size; ++i) {
			for (int j = 0; j < size; ++j) {
				c[i * size + j] = 0.0f;
			}
		}
	}

	public float consistentMultiplicationTime() {
		long startTime = System.nanoTime();

		for (int i = 0; i < size; ++i) {
			for (int j = 0; j < size; ++j) {
				float temp = 0.0f;
				for (int k = 0; k < size; ++k) {
					temp += a[i * size + k] * b[k * size + j];
				}
				c[i * size + j] = temp;
			}
		}

		long estimatedTime = System.nanoTime() - startTime;

		return (float)estimatedTime / 1000000000.0f;
	}


	public float tapeCircuitMultiplicationTime() {
	    long startTime = System.nanoTime();

        ForkJoinPool forkJoinPool = new ForkJoinPool(worldSize);
        
        ForkJoinTask[] forkJoinTask = new ForkJoinTask[worldSize];
        
		int current_size = (size / worldSize);

		class Action {
      		void act(int index) {
				int begin = index * current_size;
				int end = begin + current_size;
				forkJoinTask[index] = forkJoinPool.submit(() -> {
					for (int i = begin; i < end; ++i)
					{
						for (int j = 0; j < size; ++j)
						{
							float temp = 0.0f;
							for (int k = 0; k < size; ++k)
							{
								temp += a[i * size + k] * b[k * size + j];
							}
							c[i * size + j] = temp;
						}
					}
				});
			}
		}
		Action action = new Action();
		for (int i = 0; i < worldSize; ++i) {
			action.act(i);
		}
		
		for (int i = 0; i < worldSize; ++i) {
			forkJoinTask[i].join();
		}
		
		long estimatedTime = System.nanoTime() - startTime;

		return (float)estimatedTime / 1000000000.0f;
	}
}

public class HelloWorld{

     public static void main(String []args){
		MatrixMultiplication matrixMultiplication = new MatrixMultiplication();

        System.out.println(matrixMultiplication.consistentMultiplicationTime());
        System.out.println(matrixMultiplication.tapeCircuitMultiplicationTime());
     }
}