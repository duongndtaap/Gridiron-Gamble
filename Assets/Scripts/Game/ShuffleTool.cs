using System.Collections.Generic;

public static class ShuffleTool {
    public static List<int> ArrangeList(List<int> inputList) {
        int n = inputList.Count;
        List<int> result = new List<int>(new int[n]);

        for (int i = 0; i < n; i++) {
            result[i] = -1;
        }

        List<int> availableNumbers = new List<int>(inputList);
        System.Random random = new System.Random();

        for (int i = 0; i < n && availableNumbers.Count > 0; i++) {
            if (result[i] == -1) {
                List<int> validNumbers = new List<int>();
                for (int j = 0; j < availableNumbers.Count; j++) {
                    if (availableNumbers[j] != i && (i >= n || availableNumbers[j] != inputList[i])) {
                        validNumbers.Add(availableNumbers[j]);
                    }
                }

                int selectedNumber;
                if (validNumbers.Count > 0) {
                    int randomIndex = random.Next(0, validNumbers.Count);
                    selectedNumber = validNumbers[randomIndex];
                }
                else {
                    int randomIndex = random.Next(0, availableNumbers.Count);
                    selectedNumber = availableNumbers[randomIndex];
                }

                result[i] = selectedNumber;
                availableNumbers.Remove(selectedNumber);
            }
        }

        return result;
    }
}